import RAIN.Action;
import RAIN.Core;

@RAINDecision
class DecisionTemplate_JS extends RAIN.Action.RAINDecision
{
    private var _lastRunning:int = 0;

    function Start(ai:AI):void
	{
	    super.Start(ai);

        _lastRunning = 0;
    }

    function Execute(ai:AI):ActionResult
    {
        var tResult:ActionResult = ActionResult.SUCCESS;

        for ( ; _lastRunning < _children.Count; _lastRunning++)
        {
            tResult = _children[_lastRunning].Run(ai);
            if (tResult != ActionResult.SUCCESS)
                break;
        }

        return tResult;
    }

    function Stop(ai:AI):void
    {
        super.Stop(ai);
    }
}