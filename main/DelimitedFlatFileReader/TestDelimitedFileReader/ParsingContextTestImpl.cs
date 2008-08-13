using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader;

namespace TestDelimitedFileReader
{

    class ParsingContextTestImpl : IParsingContext
    {
        IParsingState currentState = null;
        System.Text.StringBuilder currentText = new System.Text.StringBuilder();


        public IParsingState LastState
        {
            get { return currentState; }
        }

        public string CurrentText
        {
            get { return this.currentText.ToString(); }
        }

        public void ResetText()
        {
            this.currentText.Length = 0;
        }

        #region IParsingContext Members

        public IParsingState CurrentState
        {
            get { return this.currentState; }
            set { this.currentState = value; }
        }

        public void TransitionTo(IParsingState nextState)
        {
            this.currentState = nextState;
        }

        public void Append(char ch)
        {
            this.currentText.Append(ch);
        }

        public void Append(string text)
        {
            this.currentText.Append(text);
        }

        #endregion
    }

}
