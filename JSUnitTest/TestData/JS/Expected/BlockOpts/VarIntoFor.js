﻿function test1(a,b){for(var c=1,d=10;c<d;++c)b+=c*d}function test2(a,b){var c,d;for(c=1,d=10,a=2;c<d;++c)b+=c*d}function test3(a,b){for(var c=a,c=1,d=10;c<d;++c)b+=c*d}function test4(a,b){function foo(){c=a+b}var c;for(foo();c<10;++c)b+=a}function test5(a,b){function foo(){c=a+b}var c;for(foo(),b="";c<10;++c)b+=a}function test6(a,b){for(var c=a/b;c<a;++c)b+=c}