﻿function test1(){var foo=10,bar,ack,trap;return function foo(count){--count>0&&foo(count)}(10),alert(foo),bar=function(){},function local_one_self_ref(count){--count>0&&local_one_self_ref(count)}(10),ack=function(){},ack.bar=10,trap=function trap(){trap()},trap.bar=11,bar()}function test1(){var a1,b=function(){}}function test2(){var b=function a2(){alert(a2)},a2}function test3(){var b=function a3(){alert(a3)},c=function(){},d=function(){}}function test4(){var b=function(){},c=function(){},d=function(){},a4=10;b==c&&alert(a4)}function test5(){var a5=function(){}}function test6(){var a6=function a6(x){x==6&&a6(-1)};alert(a6)}var foo=function(){},ack,trap;foo=function global_one_self_ref(count){--count>0&&global_one_self_ref(count)},ack=function(){},ack.bar=12,trap=function trap(){trap()},trap.bar=13