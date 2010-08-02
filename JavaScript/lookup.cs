// lookup.cs
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

using System;

namespace Microsoft.Ajax.Utilities
{
    public enum ReferenceType
    {
        Variable,
        Function,
        Constructor
    }


    public sealed class Lookup : AstNode
    {
        private JSLocalField m_localField;// = null; // set during analyze
        public JSLocalField LocalField
        {
            get { return m_localField; }
            set { m_localField = value; }
        }

        private bool m_isGenerated;
        internal bool IsGenerated
        {
            set { m_isGenerated = value; }
        }

        private ReferenceType m_refType = ReferenceType.Variable; // default to variable
        public ReferenceType RefType
        {
            get { return m_refType; }
        }

        private string m_name;
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                if (m_localField == null)
                {
                    m_name = value;
                }
                else
                {
                    m_localField.CrunchedName = value;
                }
            }
        }

        // this constructor is invoked when there has been a parse error. The typical scenario is a missing identifier.
        public Lookup(String name, Context context, JSParser parser)
            : base(context, parser)
        {
            m_name = name;
        }

        public override AstNode Clone()
        {
            Lookup clone = new Lookup(m_name, (Context == null ? null : Context.Clone()), Parser);
            clone.IsGenerated = m_isGenerated;
            return clone;
        }

        public override string ToCode(ToCodeFormat format)
        {
            // if we have a local field pointer that has a crunched name,
            // the return the crunched name. Otherwise just return our given name;
            return (
              m_localField != null
              ? m_localField.ToString()
              : m_name
              );
        }

        internal override string GetFunctionGuess(AstNode target)
        {
            // return the source name
            return m_name;
        }

        internal override void AnalyzeNode()
        {
            // figure out if our reference type is a function or a constructor
            if (Parent is CallNode)
            {
                m_refType = (
                  ((CallNode)Parent).IsConstructor
                  ? ReferenceType.Constructor
                  : ReferenceType.Function
                  );
            }

            ActivationObject scope = ScopeStack.Peek();
            JSVariableField variableField = scope.FindReference(m_name);
            if (variableField == null)
            {
                // this must be a global. if it isn't in the global space, throw an error
                // this name is not in the global space.
                // if it isn't generated, then we want to throw an error
                // we also don't want to report an undefined variable if it is the object
                // of a typeof operator
                if (!m_isGenerated && !(Parent is TypeOfNode))
                {
                    // report this undefined reference
                    Context.ReportUndefined(this);

                    // possibly undefined global (but definitely not local)
                    Context.HandleError(
                      (Parent is CallNode && ((CallNode)Parent).Function == this ? JSError.UndeclaredFunction : JSError.UndeclaredVariable),
                      null,
                      false
                      );
                }

                if (!(scope is GlobalScope))
                {
                    // add it to the scope so we know this scope references the global
                    scope.AddField(new JSGlobalField(
                      m_name,
                      Missing.Value,
                      0
                      ));
                }
            }
            else
            {
                // BUT if this field is a place-holder in the containing scope of a named
                // function expression, then we need to throw an ambiguous named function expression
                // error because this could cause problems.
                // OR if the field is already marked as ambiguous, throw the error
                if (variableField.NamedFunctionExpression != null
                    || variableField.IsAmbiguous)
                {
                    // mark it as a field that's referenced ambiguously
                    variableField.IsAmbiguous = true;
                    // throw as an error
                    Context.HandleError(JSError.AmbiguousNamedFunctionExpression, true);

                    // if we are preserving function names, then we need to mark this field
                    // as not crunchable
                    if (Parser.Settings.PreserveFunctionNames)
                    {
                        variableField.CanCrunch = false;
                    }
                }

                // see if this scope already points to this name
                if (scope[m_name] == null)
                {
                    // create an inner reference so we don't keep walking up the scope chain for this name
                    variableField = scope.CreateInnerField(variableField);
                }

                // add the reference
                variableField.AddReference(scope);

                // save the local field if it is one
                m_localField = variableField as JSLocalField;
            }
        }

        internal void SetOuterLocalField(ActivationObject parentScope)
        {
            // if we're trying to set the outer local field using a global scope,
            // then ignore this request. This should only do something for scopes with
            // local variables
            if (!(parentScope is GlobalScope))
            {
                // set the parent scope if it isn't the same
                if (ParentScope != parentScope)
                {
                    ParentScope = parentScope;
                }

                // get the field reference for this lookup value
                JSVariableField variableField = parentScope.FindReference(m_name);
                if (variableField != null)
                {
                    // see if this scope already points to this name
                    if (parentScope[m_name] == null)
                    {
                        // create an inner reference so we don't keep walking up the scope chain for this name
                        variableField = parentScope.CreateInnerField(variableField);
                    }

                    // save the local field
                    m_localField = variableField as JSLocalField;
                    // add a reference
                    if (m_localField != null)
                    {
                        m_localField.AddReference(parentScope);
                    }
                }
            }
        }

        internal override bool IsDebuggerStatement
        {
            get
            {
                switch (m_name)
                {
                    // lookups for these objects will pop positive for a "Debug" statement
                    case "Debug":
                    case "$Debug":
                    case "WAssert":
                        return true;

                    // everything else is okay by default
                    default:
                        return false;
                }
            }
        }

        //code in parser relies on this.name being returned from here
        public override String ToString()
        {
            return m_name;
        }
    }
}
