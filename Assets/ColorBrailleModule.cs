using System;
using ColorBraille;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;
using Rnd = UnityEngine.Random;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

/// <summary>
/// On the Subject of Color Braille
/// Created by Timwi
/// </summary>
public class ColorBrailleModule : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public KMAudio Audio;
    public KMRuleSeedable RuleSeedable;
    public KMSelectable MainSelectable; // Children are in READING order
    public KMColorblindMode ColorblindMode;

    public MeshRenderer[] DotRenderers; // BRAILLE order
    public TextMesh[] ColorblindIndicators; // BRAILLE order
    public Material[] Colors;

    public GameObject DotsParent;
    public Mesh RoundedCylinder;

    private static int _moduleIdCounter = 1;
    private int _moduleId;
    private int _correctButtonToPress;
    private bool _isSolved = false;
    private int[] _colorIxs;    // BRAILLE order
    private bool _colorblind;
    private static readonly string _colorNames = "KBGCRMYW";
    private static readonly bool[] _cbBlack = new[] { false, false, true, true, true, true, true, true };

    private const int _numLetters = 5;

    struct MangledWordInfo
    {
        public int[] MangledWord;
        public Mangling Mangling;
    }

    void Start()
    {
        _moduleId = _moduleIdCounter++;

        var threeWords = WordsData.Words.Keys.ToList().Shuffle().Take(3).ToArray();
        Debug.LogFormat(@"[Color Braille #{0}] The red word is: {1}", _moduleId, threeWords[0].ToUpperInvariant());
        Debug.LogFormat(@"[Color Braille #{0}] The green word is: {1}", _moduleId, threeWords[1].ToUpperInvariant());
        Debug.LogFormat(@"[Color Braille #{0}] The blue word is: {1}", _moduleId, threeWords[2].ToUpperInvariant());

        tryAgain:
        var mangledChannel = Rnd.Range(0, 3);
        var word = WordsData.Words[threeWords[mangledChannel]];
        if (word.Length != _numLetters)
            throw new InvalidOperationException();
        var mangledWords = new List<MangledWordInfo>();
        var rowIx = 0;

        foreach (var mangling in (Mangling[]) Enum.GetValues(typeof(Mangling)))
        {
            var mangledWord = word.ToArray();
            switch (mangling)
            {
                case Mangling.MiddleRowShiftedToTheRight:
                    rowIx = 1;
                    goto case Mangling.TopRowShiftedToTheRight;
                case Mangling.BottomRowShiftedToTheRight:
                    rowIx = 2;
                    goto case Mangling.TopRowShiftedToTheRight;
                case Mangling.TopRowShiftedToTheRight:
                {
                    var row = Enumerable.Range(0, _numLetters).SelectMany(ltr => new[] { (word[ltr] & (1 << (0 + rowIx))) != 0, (word[ltr] & (1 << (3 + rowIx))) != 0 }).ToArray();
                    if (row[2 * _numLetters - 1])
                        continue;
                    for (var ltr = 0; ltr < _numLetters; ltr++)
                    {
                        mangledWord[ltr] &= (0xdb >> (2 - rowIx));
                        if (ltr != 0 && row[2 * ltr - 1])
                            mangledWord[ltr] |= (1 << (0 + rowIx));
                        if (row[2 * ltr])
                            mangledWord[ltr] |= (1 << (3 + rowIx));
                    }
                }
                break;

                case Mangling.MiddleRowShiftedToTheLeft:
                    rowIx = 1;
                    goto case Mangling.TopRowShiftedToTheLeft;
                case Mangling.BottomRowShiftedToTheLeft:
                    rowIx = 2;
                    goto case Mangling.TopRowShiftedToTheLeft;
                case Mangling.TopRowShiftedToTheLeft:
                {
                    var row = Enumerable.Range(0, _numLetters).SelectMany(ltr => new[] { (word[ltr] & (1 << (0 + rowIx))) != 0, (word[ltr] & (1 << (3 + rowIx))) != 0 }).ToArray();
                    if (row[0])
                        continue;
                    for (var ltr = 0; ltr < _numLetters; ltr++)
                    {
                        mangledWord[ltr] &= (0xdb >> (2 - rowIx));
                        if (row[2 * ltr + 1])
                            mangledWord[ltr] |= (1 << (0 + rowIx));
                        if (ltr != _numLetters - 1 && row[2 * ltr + 2])
                            mangledWord[ltr] |= (1 << (3 + rowIx));
                    }
                }
                break;

                case Mangling.EachLetterUpsideDown:
                    for (var ltr = 0; ltr < _numLetters; ltr++)
                        mangledWord[ltr] =
                            ((word[ltr] & (1 << 0)) << 5) |
                            ((word[ltr] & (1 << 1)) << 3) |
                            ((word[ltr] & (1 << 2)) << 1) |
                            ((word[ltr] & (1 << 3)) >> 1) |
                            ((word[ltr] & (1 << 4)) >> 3) |
                            ((word[ltr] & (1 << 5)) >> 5);
                    break;

                case Mangling.EachLetterHorizontallyFlipped:
                    for (var ltr = 0; ltr < _numLetters; ltr++)
                        mangledWord[ltr] =
                            ((word[ltr] & (1 << 0)) << 3) |
                            ((word[ltr] & (1 << 1)) << 3) |
                            ((word[ltr] & (1 << 2)) << 3) |
                            ((word[ltr] & (1 << 3)) >> 3) |
                            ((word[ltr] & (1 << 4)) >> 3) |
                            ((word[ltr] & (1 << 5)) >> 3);
                    break;

                case Mangling.EachLetterVerticallyFlipped:
                    for (var ltr = 0; ltr < _numLetters; ltr++)
                        mangledWord[ltr] =
                            ((word[ltr] & (1 << 0)) << 2) |
                            ((word[ltr] & (1 << 1))) |
                            ((word[ltr] & (1 << 2)) >> 2) |
                            ((word[ltr] & (1 << 3)) << 2) |
                            ((word[ltr] & (1 << 4))) |
                            ((word[ltr] & (1 << 5)) >> 2);
                    break;

                case Mangling.DotsAreInverted:
                    for (var ltr = 0; ltr < _numLetters; ltr++)
                        mangledWord[ltr] = (~word[ltr]) & 0x3f;
                    break;
            }
            mangledWords.Add(new MangledWordInfo { MangledWord = mangledWord, Mangling = mangling });
        }

        mangledWords.RemoveAll(w => mangledWords.Count(mw => mw.MangledWord.SequenceEqual(w.MangledWord)) > 1 || WordsData.Words.Any(kvp => kvp.Value.SequenceEqual(w.MangledWord)));
        if (mangledWords.Count == 0)
            goto tryAgain;

        var chosenWord = mangledWords.PickRandom();

        Debug.LogFormat(@"[Color Braille #{0}] The mangled channel is: {1}", _moduleId, new[] { "red", "green", "blue" }[mangledChannel]);
        Debug.LogFormat(@"[Color Braille #{0}] The mangling is: {1}", _moduleId, chosenWord.Mangling);
        Debug.LogFormat(@"[Color Braille #{0}] The unmangled braille is: {1}", _moduleId, toBrailleNumbers(word));
        Debug.LogFormat(@"[Color Braille #{0}] The mangled braille is: {1}", _moduleId, toBrailleNumbers(chosenWord.MangledWord));

        var displayedWords = Enumerable.Range(0, 3).Select(ch => ch == mangledChannel ? chosenWord.MangledWord : WordsData.Words[threeWords[ch]]).ToArray();

        var svgColorNames = "#000,#00f,#0f0,#0ff,#f00,#f0f,#ff0,#fff".Split(',');
        var svg = new StringBuilder();
        _colorIxs = new int[6 * _numLetters];
        for (var i = 0; i < _numLetters * 6; i++)
        {
            var red = (displayedWords[0][i / 6] & (1 << (i % 6))) != 0;
            var green = (displayedWords[1][i / 6] & (1 << (i % 6))) != 0;
            var blue = (displayedWords[2][i / 6] & (1 << (i % 6))) != 0;
            var colorIx = (red ? 4 : 0) + (green ? 2 : 0) + (blue ? 1 : 0);
            _colorIxs[i] = colorIx;
            svg.AppendFormat(@"<circle fill='{0}' cx='{1}' cy='{2}' r='.007' />", svgColorNames[colorIx], -.07 + (.2 / 4 * (i / 6)) + ((i / 3) % 2 == 0 ? -.01 : .01), (i % 3) * .02);
        }

        Debug.LogFormat(@"[Color Braille #{0}]=svg[Module:]<svg xmlns='http://www.w3.org/2000/svg' viewBox='-.089 -.009 .238 .058' stroke='black' stroke-width='.001'>{1}</svg>", _moduleId, svg.ToString());

        for (var i = 0; i < MainSelectable.Children.Length; i++)
            MainSelectable.Children[i].OnInteract = getLedHandler(i);


        // RULE SEED STARTS HERE
        var rnd = RuleSeedable.GetRNG();
        Debug.LogFormat(@"[Color Braille #{0}] Using rule seed: {1}", _moduleId, rnd.Seed);
        var poss = rnd.ShuffleFisherYates(Enumerable.Range(0, 30).ToList());


        _correctButtonToPress = poss[3 * (int) chosenWord.Mangling + mangledChannel];
        Debug.LogFormat(@"[Color Braille #{0}] Correct LED to press is #{1} in reading order.", _moduleId, _correctButtonToPress + 1);

        _colorblind = ColorblindMode.ColorblindModeActive;

        foreach (var led in DotRenderers)
            led.sharedMaterial = Colors[0];
        foreach (var txt in ColorblindIndicators)
            txt.gameObject.SetActive(false);
        Module.OnActivate += delegate { StartCoroutine(animateLeds(on: true)); };
    }

    private void setColor(int ix, bool on)
    {
        DotRenderers[ix].sharedMaterial = on ? Colors[_colorIxs[ix]] : Colors[0];
        ColorblindIndicators[ix].text = on ? _colorNames[_colorIxs[ix]].ToString() : "";
        ColorblindIndicators[ix].color = _cbBlack[_colorIxs[ix]] ? Color.black : Color.white;
        ColorblindIndicators[ix].gameObject.SetActive(_colorblind && on);
    }

    private IEnumerator animateLeds(bool on)
    {
        yield return null;

        for (var x = 0; x < 2 * _numLetters + 2; x++)
        {
            if (x < 2 * _numLetters)
            {
                setColor(3 * x, on);
                yield return new WaitForSeconds(.03f);
            }
            if (x > 0 && x < 2 * _numLetters + 1)
            {
                setColor(3 * (x - 1) + 1, on);
                yield return new WaitForSeconds(.03f);
            }
            if (x > 1)
            {
                setColor(3 * (x - 2) + 2, on);
                yield return new WaitForSeconds(.03f);
            }
            yield return new WaitForSeconds(.07f);
        }
    }

    private KMSelectable.OnInteractHandler getLedHandler(int i)
    {
        return delegate
        {
            MainSelectable.Children[i].AddInteractionPunch();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, MainSelectable.Children[i].transform);

            if (_isSolved)
                return false;
            if (i == _correctButtonToPress)
            {
                Debug.LogFormat(@"[Color Braille #{0}] You pressed LED #{1}. Correct!", _moduleId, i + 1);
                Module.HandlePass();
                _isSolved = true;
                StartCoroutine(animateLeds(on: false));
            }
            else
            {
                Debug.LogFormat(@"[Color Braille #{0}] You pressed LED #{1}. Strike!", _moduleId, i + 1);
                Module.HandleStrike();
            }
            return false;
        };
    }

    static string toBrailleNumbers(int[] bitPatterns)
    {
        return bitPatterns.Select(bitPattern => toBrailleNumbers(bitPattern)).Join("; ");
    }

    static string toBrailleNumbers(int bitPattern)
    {
        return Enumerable.Range(0, 6).Select(bit => (bitPattern & (1 << bit)) != 0 ? (bit + 1).ToString() : "").Join("");
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} 5 [press the 5th LED in reading order] | !{0} colorblind [enable color-blind mode]";
#pragma warning restore 414

    KMSelectable[] ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*(color[- ]*blind|cb)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) && !_isSolved)
        {
            _colorblind = true;
            StartCoroutine(animateLeds(true));
            return new KMSelectable[0];
        }
        var m = Regex.Match(command, @"^\s*(\d+)\s*$");
        int ix;
        if (m.Success && int.TryParse(m.Groups[1].Value, out ix) && ix >= 1 && ix <= 30)
            return new[] { MainSelectable.Children[ix - 1] };
        return null;
    }

    void TwitchHandleForcedSolve()
    {
        if (!_isSolved)
            MainSelectable.Children[_correctButtonToPress].OnInteract();
    }
}