﻿function foo(bar)
{
    var ext,
        ndx,
        suffix;
    if(bar)
    {
        for(ext = bar,ndx = 0; ndx < 10; ++ndx)
        {
            switch(ndx)
            {
                case 2:
                    suffix = "x";
                    break;
                case 4:
                    suffix = "4";
                    break;
                case 6:
                    suffix = "6";
                    break;
                default:
                    suffix = "-"
            }
            ext += suffix
        }
        return ext
    }
}