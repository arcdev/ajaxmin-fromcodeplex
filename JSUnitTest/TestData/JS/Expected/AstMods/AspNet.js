﻿function foo(){if('<%= Request.QueryString["foo"] %>'!=""){var bar="WOW-wee!";return alert(bar),0}return 1}var bar='wow<%= Request.QueryString["foo"] %>wee',check='<%= Request.QueryString["foo"] %>'<42,mult='<%= Request.QueryString["foo"] %>'*16;