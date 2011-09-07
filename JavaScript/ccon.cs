﻿// ccon.cs
//
// Copyright 2010 Microsoft Corporation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Microsoft.Ajax.Utilities
{
    public class ConditionalCompilationOn : ConditionalCompilationStatement
    {
        public ConditionalCompilationOn(Context context, JSParser parser)
            : base(context, parser)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            if (visitor != null)
            {
                visitor.Visit(this);
            }
        }

        public override string ToCode(ToCodeFormat format)
        {
            var code = string.Empty;
            
            // if we haven't output a cc_on yet, or if we haven't allowed redundants to be removed...
            if (!Parser.OutputCCOn 
                || !Parser.Settings.IsModificationAllowed(TreeModifications.RemoveUnnecessaryCCOnStatements))
            {
                Parser.OutputCCOn = true;
                code = "@cc_on";
            }

            return code;
        }
    }
}
