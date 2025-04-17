using Cryptoquip.Models;
using Cryptoquip.Services;

namespace CryptoquipConsole;

class Program
{
    static void Main(string[] args)
    {
        const string puzzle = "X KYP Y DSSW SF EYEEG'K BYVU LSEYG YFE YDD SB Y KHEEUF X BSHFE CGKUDB DSSWXFN YL RXC FSL YK CG EYEEG OHL YK YFSLRUZ CYF. CYGOU X'C NZSPXFN HQ Y DXLLDU. CYGOU XL PYK LRU QYXF XF RXK BYVU. OHL XL VYCU LS CU LRYL BSZ YDD LRU QDUYKHZU CSCCY YFE EYEEG BXFE XF HK VRXDEZUF, LRUZU YZU SLRUZ LXCUK XL CHKL OU RUYZLOZUYWXFN LS LRUC.";
        Solver solver = new();
        solver.RunSolver(LogMessage, new DecoderRingCustom(), new WordList(), puzzle, true);
    }
    
    private static void LogMessage(string message)
    {
        Console.WriteLine(message);
    }
}