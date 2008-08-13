using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader
{
    internal class FieldParser : IParsingContext
    {
        // The maximum number of characters we could have in a single field is fixed to 64K.
        // We might want to parametrize this later by exposing it in a component property.
        public const int ParsingBufferMaxSize = 65536;

        IParsingState initialParsingState = null;
        IParsingState currentParsingState = null;
        IParsingState rowDelimiterState = null;

        StringBuilder currentText = new StringBuilder();

        private FieldParser() 
        {
        }

        public bool RowDelimiterMatch
        {
            get
            {
                return this.currentParsingState == this.rowDelimiterState;
            }
        }

        public string CurrentText
        {
            get { return this.currentText.ToString(); }
        }

        public void ParseNext(IFileReader reader)
        {
            this.ParseNext(reader, null);
        }

        public void ParseNext(IFileReader reader, IRowParsingContext rowParsingContext)
        {
            ResetParsingState();

            ParsingResult parsingResult = ParsingResult.Match;
            char currentChar = '\0';

            do
            {
                if (currentText.Length >= ParsingBufferMaxSize)
                {
                    ParsingBufferOverflowException ex = new ParsingBufferOverflowException();
                    if (rowParsingContext != null)
                    {
                        ex.ColumnIndex = rowParsingContext.ColumnCount;
                    }
                    throw ex;
                }

                if (parsingResult != ParsingResult.Miss)
                {
                    currentChar = reader.GetNextChar();
                    if (reader.IsEOF)
                    {
                        break;
                    }
                    else if (rowParsingContext != null)
                    {
                        rowParsingContext.Append(currentChar);
                    }
                }

                if (this.currentParsingState != null)
                {
                    parsingResult = currentParsingState.ProcessCharacter(this, currentChar);
                }
                else
                {
                    // We should not get into this state for our parsing.
                    // It would mean our graph of states is not connected properly.
                    throw new FieldParsingException(MessageStrings.BadParsingGraphError);
                }
            }
            while (parsingResult != ParsingResult.Done);
        }

        public void ResetParsingState()
        {
            this.currentParsingState = initialParsingState;
            this.currentText.Length = 0;
        }

        public static FieldParser BuildParserWithSingleDelimiter(string delimiter)
        {
            ArgumentVerifier.CheckStringArgument(delimiter, "delimiter");
            
            FieldParser parser = new FieldParser();
            
            parser.initialParsingState = new ParsingState();

            MatchTokenParsingState delimiterState = new MatchTokenParsingState(delimiter);

            parser.initialParsingState.AddNextState(delimiterState);

            delimiterState.SetErrorState(parser.initialParsingState);

            return parser;
        }

        public static FieldParser BuildParserWithTwoDelimiters(string delimiter, string rowDelimiter)
        {
            ArgumentVerifier.CheckStringArgument(delimiter, "delimiter");
            ArgumentVerifier.CheckStringArgument(rowDelimiter, "rowDelimiter");

            FieldParser parser = new FieldParser();

            parser.initialParsingState = new ParsingState();

            parser.rowDelimiterState = new MatchTokenParsingState(rowDelimiter);
            MatchTokenParsingState delimiterState = new MatchTokenParsingState(delimiter);

            parser.initialParsingState.AddNextState(parser.rowDelimiterState);
            parser.initialParsingState.AddNextState(delimiterState);

            parser.rowDelimiterState.SetErrorState(parser.initialParsingState);

            delimiterState.SetErrorState(parser.initialParsingState);

            return parser;
        }

        public static FieldParser BuildParserWithSingleDelimiterAndQualifier(string delimiter, string qualifier)
        {
            ArgumentVerifier.CheckStringArgument(delimiter, "delimiter");
            ArgumentVerifier.CheckStringArgument(qualifier, "qualifier");

            FieldParser parser = new FieldParser();

            parser.initialParsingState = new ParsingState();
            ParsingState fieldState = new ParsingState();
            ParsingState qualifiedFieldState = new ParsingState();

            MatchTokenParsingState firstQualifierState = new MatchTokenParsingState(qualifier);
            MatchTokenParsingState delimiterState = new MatchTokenParsingState(delimiter);
            MatchEscapedTokenParsingState secondQualifierState = new MatchEscapedTokenParsingState(qualifier);

            parser.initialParsingState.AddNextState(firstQualifierState);
            parser.initialParsingState.AddNextState(fieldState);

            fieldState.AddNextState(delimiterState);

            qualifiedFieldState.AddNextState(secondQualifierState);

            firstQualifierState.AddNextState(qualifiedFieldState);
            firstQualifierState.SetErrorState(fieldState);

            delimiterState.SetErrorState(fieldState);

            secondQualifierState.AddNextState(fieldState);
            secondQualifierState.SetErrorState(qualifiedFieldState);

            return parser;
        }

        public static FieldParser BuildParserWithTwoDelimitersAndQualifier(string delimiter, string rowDelimiter, string qualifier)
        {
            ArgumentVerifier.CheckStringArgument(delimiter, "delimiter");
            ArgumentVerifier.CheckStringArgument(rowDelimiter, "rowDelimiter");
            ArgumentVerifier.CheckStringArgument(qualifier, "qualifier");

            FieldParser parser = new FieldParser();

            parser.initialParsingState = new ParsingState();
            ParsingState fieldState = new ParsingState();
            ParsingState qualifiedFieldState = new ParsingState();

            MatchTokenParsingState firstQualifierState = new MatchTokenParsingState(qualifier);
            parser.rowDelimiterState = new MatchTokenParsingState(rowDelimiter);
            MatchTokenParsingState delimiterState = new MatchTokenParsingState(delimiter);
            MatchEscapedTokenParsingState secondQualifierState = new MatchEscapedTokenParsingState(qualifier);

            parser.initialParsingState.AddNextState(firstQualifierState);
            parser.initialParsingState.AddNextState(fieldState);

            fieldState.AddNextState(parser.rowDelimiterState);
            fieldState.AddNextState(delimiterState);

            qualifiedFieldState.AddNextState(secondQualifierState);

            firstQualifierState.AddNextState(qualifiedFieldState);
            firstQualifierState.SetErrorState(fieldState);

            parser.rowDelimiterState.SetErrorState(fieldState);

            delimiterState.SetErrorState(fieldState);

            secondQualifierState.AddNextState(fieldState);
            secondQualifierState.SetErrorState(qualifiedFieldState);

            return parser;
        }

        #region IParsingContext Members

        public IParsingState CurrentState
        {
            get { return this.currentParsingState; }
            set { this.currentParsingState = value; }
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
