var t=new function(){this.foo=function(){return function(){return"foo"}};this.bar=function(){return function(){return"bar"}};var x=function(){return"woof!"}.ToString(),y=function(){return"bark!"}.ToString(),p=function(){return this}(),q=function(){return this}(),z=setTimeout(function(){return"foo"},5e3)},addEvent;t.foo(),function(p){return p*10}.as("foo"),function(){alert("ralph")}(),function(){return!0}()==!0?alert("true"):alert("false");addEvent=function(){function ie_addEvent(el,evt,fn){el.attachEvent("on"+evt,fn)}function w3c_addEvent(el,evt,fn,useCap){el.addEventListener(evt,fn,useCap)}return typeof window.addEventListener!="undefined"?w3c_addEvent:typeof window.attachEvent!="undefined"?ie_addEvent:void 0}();String.addMethod("trim",function(){return this})