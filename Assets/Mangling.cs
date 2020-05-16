using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorBraille
{
    enum Mangling
    {
        TopRowShiftedToTheRight,
        TopRowShiftedToTheLeft,
        MiddleRowShiftedToTheRight,
        MiddleRowShiftedToTheLeft,
        BottomRowShiftedToTheRight,
        BottomRowShiftedToTheLeft,
        EachLetterUpsideDown,
        EachLetterHorizontallyFlipped,
        EachLetterVerticallyFlipped,
        DotsAreInverted
    }
}
