using Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

class ParsingStateTestImpl : IParsingState
{
    ParsingResult result = ParsingResult.Match;

    public ParsingStateTestImpl(ParsingResult result)
    {
        this.result = result;
    }

    #region IParsingState Members

    public void AddNextState(IParsingState nextState)
    {
    }

    public void SetErrorState(IParsingState errorState)
    {
    }

    public ParsingResult ProcessCharacter(IParsingContext context, char nextChar)
    {
        return result;
    }

    #endregion
}

