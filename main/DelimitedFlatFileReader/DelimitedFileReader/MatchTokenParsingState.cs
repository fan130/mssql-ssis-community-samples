using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader
{
    internal class MatchTokenParsingState : IParsingState
    {
        protected string tokenToMatch = string.Empty;

        protected int currentCharacterIndex = 0;

        protected IParsingState nextState = null;
        protected IParsingState errorState = null;

        public MatchTokenParsingState(string token)
        {
            ArgumentVerifier.CheckStringArgument(token, "token");

            this.tokenToMatch = token;
        }

        #region IParsingState Members

        public void AddNextState(IParsingState nextState)
        {
            this.nextState = nextState;
        }

        public void SetErrorState(IParsingState errorState)
        {
            this.errorState = errorState;
        }

        public ParsingResult ProcessCharacter(IParsingContext context, char nextChar)
        {
            ArgumentVerifier.CheckObjectArgument(context, "context");

            return ProcessCharacterImpl(context, nextChar);
        }

        #endregion

        protected virtual ParsingResult ProcessCharacterImpl(IParsingContext context, char nextChar)
        {
            ParsingResult finalResult = ParsingResult.Match;

            System.Diagnostics.Debug.Assert(currentCharacterIndex < tokenToMatch.Length);

            if (nextChar == tokenToMatch[currentCharacterIndex])
            {
                currentCharacterIndex++;
                if (this.currentCharacterIndex == tokenToMatch.Length)
                {
                    if (this.nextState != null)
                    {
                        context.CurrentState = this.nextState;
                    }
                    else
                    {
                        finalResult = ParsingResult.Done;
                    }
                    this.currentCharacterIndex = 0;
                }
            }
            else
            {
                if (currentCharacterIndex == 0)
                {
                    finalResult = ParsingResult.Miss;
                }
                else
                {
                    context.CurrentState = this.errorState;
                    context.Append(this.tokenToMatch.Substring(0, this.currentCharacterIndex));
                    finalResult = ParsingResult.Miss;
                }
                this.currentCharacterIndex = 0;
            }

            return finalResult;
        }
    }

    internal class MatchEscapedTokenParsingState : MatchTokenParsingState
    {
        public MatchEscapedTokenParsingState(string token) : base(token) { }

        protected override ParsingResult ProcessCharacterImpl(IParsingContext context, char nextChar)
        {
            ParsingResult finalResult = ParsingResult.Match;

            System.Diagnostics.Debug.Assert(currentCharacterIndex < tokenToMatch.Length * 2);

            int currentCompareCharacterIndex = currentCharacterIndex % tokenToMatch.Length;
            if (nextChar == tokenToMatch[currentCompareCharacterIndex])
            {
                currentCharacterIndex++;
                if (this.currentCharacterIndex == tokenToMatch.Length * 2)
                {
                    if (this.errorState != null)
                    {
                        context.CurrentState = this.errorState;
                        context.Append(this.tokenToMatch);
                    }
                    else
                    {
                        finalResult = ParsingResult.Done;
                    }
                    this.currentCharacterIndex = 0;
                }
            }
            else
            {
                if (currentCharacterIndex == 0)
                {
                    finalResult = ParsingResult.Miss;
                }
                else if (currentCompareCharacterIndex == currentCharacterIndex)
                {
                    context.CurrentState = this.errorState;
                    context.Append(this.tokenToMatch.Substring(0, this.currentCharacterIndex));
                    finalResult = ParsingResult.Miss;
                }
                else
                {
                    context.CurrentState = this.nextState;
                    context.Append(this.tokenToMatch.Substring(0, currentCompareCharacterIndex));
                    finalResult = ParsingResult.Miss;
                }

                this.currentCharacterIndex = 0;
            }

            return finalResult;
        }
    }
}
