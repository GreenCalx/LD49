using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TN = Trick.TRICK_NATURE;
using TT = Trick.TRICK_TYPE;
public class TrickLine
{
    public class TrickTimePair {
        public Trick trick;
        public double time;
        public TrickTimePair( Trick iTrick, double iTime)
        {
            trick   = iTrick;
            time    = iTime;
        }
        public int computeScore()
        {
            return (int)(trick.value * (1+time));
        }
    }

    public List<TrickTimePair> full_line;
    public bool is_opened;
    public TrickLine()
    {
        full_line = new List<TrickTimePair>();
        is_opened = false;
    }

    public void open(Trick iTrick)
    {
        clear();
        full_line.Add( new TrickTimePair(iTrick, 0));
        is_opened = true;
    }

    public void close()
    {

        is_opened = false;
    }

    public void clear()
    {
        full_line.Clear();
    }

    public void add(Trick iTrick, double iDurationTime)
    {
        if (!is_opened)
            return;
        
        TrickTimePair trickpair = full_line[full_line.Count-1];
        Trick last_trick = trickpair.trick;
        TN last_trick_nat = last_trick.nature;

        Trick neutralChain  = tryNeutralChain( iTrick, last_trick);
        if ( neutralChain != null)
        {
            if (full_line.Count > 0)
                full_line.RemoveAt(full_line.Count-1);
            TrickTimePair ttp = new TrickTimePair( neutralChain, iDurationTime);
            full_line.Add(ttp);
            return;
        }

        
        if (( iTrick == last_trick ) && ( iTrick.isFlat()) )
        {
            // continuing same trick, update time
            full_line[full_line.Count-1].time = iDurationTime;
            return;
        } else {
            // otherwise add trick to pool
            TrickTimePair ttp = new TrickTimePair( iTrick, iDurationTime);
            full_line.Add(ttp);
        }

        tryChains(iDurationTime);

    }

    public void tryChains(double iDurationTime)
    {
        if (full_line.Count <= 1)
            return;

        bool hasChained = false;
        do {
            TrickTimePair ttp1 = full_line[full_line.Count-1];
            TrickTimePair ttp2 = full_line[full_line.Count-2];

            Trick chained = tryChain( ttp1.trick, ttp2.trick);
            
            hasChained = (chained != null);

            if (hasChained)
            {
                full_line.RemoveAt(full_line.Count-1);
                full_line.RemoveAt(full_line.Count-1);

                TrickTimePair chained_ttp = new TrickTimePair( chained, iDurationTime);
                full_line.Add(chained_ttp);
            }

        } while ((hasChained)&&(full_line.Count>1));
    }

    public Trick tryNeutralChain( Trick iT1, Trick iT2)
    {
        // NEUTRAL + NEUTRAL = NEUTRAL
        if ( iT1.isNeutral() && iT2.isNeutral() )
            return iT1;

        // FLAT + NEUTRAL = FLAT + NEUTRAL
        if ( iT1.isFlat() && iT2.isNeutral())
            return null;
        if ( iT2.isFlat() && iT1.isNeutral())
            return null;

        // NEUTRAL + X = X
        if ( iT1.isNeutral() && !iT2.isNeutral() )
            return iT2;
        else if ( !iT1.isNeutral() && iT2.isNeutral() )
            return iT1;
        
        return null;
    }

    public Trick tryChain(Trick iT1, Trick iT2)
    {
        // BASIC + BASIC = CHAINED
        if ( iT1.isBasic() && iT2.isBasic() )
        {
            Trick chained = TrickDictionary.checkChainedTricks( iT1, iT2);
            if (chained != null)
            { return chained; }
        }

        // CHAINED + BASIC
        if ( iT1.isChained() && iT2.isBasic() )
        {
            Trick chained = TrickDictionary.checkChainedTricks( iT2, iT1);
            if (chained != null)
            { return chained; }
        } 
        
        if ( iT1.isBasic() && iT2.isChained() )
        {
            Trick chained = TrickDictionary.checkChainedTricks( iT1, iT2);
            if (chained != null)
            { return chained; }     
        }

        if ( iT1.isChained() && iT2.isChained() )
        {
            Trick chained = TrickDictionary.checkChainedTricks( iT1, iT2);
            if (chained != null)
            { return chained; }     
        }

        return null;
    }

    public int getLineScore(float combo_multiplier)
    {
        int line_score = 0;
        if ( full_line.Count <= 0 )
        { return 0; }

        for ( int i=0; i < full_line.Count; i++ )
        {
            TrickTimePair trickpair = full_line[i];
            line_score += (int) (trickpair.computeScore() * (i+combo_multiplier));
        }

        return line_score;
    }

    public List<Trick> getTrickList()
    {
        List<Trick> retval = new List<Trick>();
        foreach ( TrickTimePair ttp in full_line )
        { retval.Add(ttp.trick); }
        return retval;
    }

    public TrickTimePair getLastTrick()
    {
        if ( full_line.Count <= 0 )
        { return null; }
        else
            return full_line[full_line.Count-1];
    }
}
