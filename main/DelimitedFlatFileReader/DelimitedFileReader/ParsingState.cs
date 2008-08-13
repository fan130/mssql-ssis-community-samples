using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader
{
    internal class ParsingState : IParsingState
    {
        List<IParsingState> nextStates = new List<IParsingState>();

        #region IParsingState Members

        public void AddNextState(IParsingState parsingState)
        {
            ArgumentVerifier.CheckObjectArgument(parsingState, "parsingState");

            nextStates.Add(parsingState);
        }

        public void SetErrorState(IParsingState errorState)
        {
        }

        public ParsingResult ProcessCharacter(IParsingContext context, char nextChar)
        {
            ArgumentVerifier.CheckObjectArgument(context, "context");

            foreach (IParsingState parsingState in nextStates)
            {
                ParsingResult result = parsingState.ProcessCharacter(context, nextChar);
                if (result != ParsingResult.Miss)
                {
                    if (context.CurrentState == this)
                    {
                        // If the current state has not changed in the ProcessCharacter call,
                        // we should change it now.
                        context.CurrentState = parsingState;
                    }
                    return result;
                }
            }

            context.Append(nextChar);
            return ParsingResult.Match;
        }

        #endregion
    }
}
