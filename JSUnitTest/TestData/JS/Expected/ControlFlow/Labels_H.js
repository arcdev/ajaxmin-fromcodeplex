function foo(){var i,u=0,n,r,t;n:for(n=0;n<10;++n){i="";t:for(r=0;r<n;++r){i=i+r+" ";continue t}if(n==8)break n;else if(n==4)continue n;t:for(t=0;t<n;++t){if(t==10)break n;else if(t==7)continue t;i:while(u<t){u++;continue i}if(t==n)continue;else break}document.write("<div>"+i+n+"</div>")}}