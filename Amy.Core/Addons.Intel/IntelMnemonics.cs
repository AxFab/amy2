﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Addons.Intel
{
    public static class IntelMnemonics
    {
        public const string AAA = "AAA";
        public const string AAD = "AAD";
        public const string AAM = "AAM";
        public const string AAS = "AAS";
        public const string ADC = "ADC";
        public const string ADCX = "ADCX";
        public const string ADD = "ADD";
        public const string ADDPD = "ADDPD";
        public const string ADDPS = "ADDPS";
        public const string ADDSD = "ADDSD";
        public const string ADDSS = "ADDSS";
        public const string ADDSUBPD = "ADDSUBPD";
        public const string ADDSUBPS = "ADDSUBPS";
        public const string ADOX = "ADOX";
        public const string AESDEC = "AESDEC";
        public const string AESDECLAST = "AESDECLAST";
        public const string AESENC = "AESENC";
        public const string AESENCLAST = "AESENCLAST";
        public const string AESIMC = "AESIMC";
        public const string AESKEYGENASSIST = "AESKEYGENASSIST";
        public const string AND = "AND";
        public const string ANDN = "ANDN";
        public const string ANDNPD = "ANDNPD";
        public const string ANDNPS = "ANDNPS";
        public const string ANDPD = "ANDPD";
        public const string ANDPS = "ANDPS";
        public const string ARPL = "ARPL";
        public const string BEXTR = "BEXTR";
        public const string BLENDPD = "BLENDPD";
        public const string BLENDPS = "BLENDPS";
        public const string BLENDVPD = "BLENDVPD";
        public const string BLENDVPS = "BLENDVPS";
        public const string BLSI = "BLSI";
        public const string BLSMSK = "BLSMSK";
        public const string BLSR = "BLSR";
        public const string BNDCL = "BNDCL";
        public const string BNDCN = "BNDCN";
        public const string BNDCU = "BNDCU";
        public const string BNDLDX = "BNDLDX";
        public const string BNDMK = "BNDMK";
        public const string BNDMOV = "BNDMOV";
        public const string BNDSTX = "BNDSTX";
        public const string BOUND = "BOUND";
        public const string BSF = "BSF";
        public const string BSR = "BSR";
        public const string BSWAP = "BSWAP";
        public const string BT = "BT";
        public const string BTC = "BTC";
        public const string BTR = "BTR";
        public const string BTS = "BTS";
        public const string BZHI = "BZHI";
        public const string CALL = "CALL";
        public const string CBW = "CBW";
        public const string CDQE = "CDQE";
        public const string CLAC = "CLAC";
        public const string CLC = "CLC";
        public const string CLD = "CLD";
        public const string CLFLUSH = "CLFLUSH";
        public const string CLI = "CLI";
        public const string CLTS = "CLTS";
        public const string CMC = "CMC";
        public const string CMOVcc = "CMOVcc";
        public const string CMP = "CMP";
        public const string CMPPD = "CMPPD";
        public const string CMPPS = "CMPPS";
        public const string CMPS = "CMPS";
        public const string CMPSB = "CMPSB";
        public const string CMPSD = "CMPSD";
        public const string CMPSQ = "CMPSQ";
        public const string CMPSS = "CMPSS";
        public const string CMPSW = "CMPSW";
        public const string CMPXCHG = "CMPXCHG";
        public const string CMPXCHG816B = "CMPXCHG816B";
        public const string CMPXCHG8B = "CMPXCHG8B";
        public const string COMISD = "COMISD";
        public const string COMISS = "COMISS";
        public const string CPUID = "CPUID";
        public const string CRC32 = "CRC32";
        public const string CVTDQ2PD = "CVTDQ2PD";
        public const string CVTDQ2PS = "CVTDQ2PS";
        public const string CVTPD2DQ = "CVTPD2DQ";
        public const string CVTPD2PI = "CVTPD2PI";
        public const string CVTPD2PS = "CVTPD2PS";
        public const string CVTPI2PD = "CVTPI2PD";
        public const string CVTPI2PS = "CVTPI2PS";
        public const string CVTPS2DQ = "CVTPS2DQ";
        public const string CVTPS2PD = "CVTPS2PD";
        public const string CVTPS2PI = "CVTPS2PI";
        public const string CVTSD2SI = "CVTSD2SI";
        public const string CVTSD2SS = "CVTSD2SS";
        public const string CVTSI2SD = "CVTSI2SD";
        public const string CWDE = "CWDE";
        public const string DAA = "DAA";
        public const string DAS = "DAS";
        public const string DEC = "DEC";
        public const string DIV = "DIV";
        public const string DIVPD = "DIVPD";
        public const string DIVPS = "DIVPS";
        public const string DIVSD = "DIVSD";
        public const string DIVSS = "DIVSS";
        public const string DPPD = "DPPD";
        public const string DPPS = "DPPS";
        public const string EMMS = "EMMS";
        public const string ENTER = "ENTER";
        public const string EXTRACTPS = "EXTRACTPS";
        public const string F2XM1 = "F2XM1";
        public const string FABS = "FABS";
        public const string FADD = "FADD";
        public const string FADDP = "FADDP";
        public const string FBLD = "FBLD";
        public const string FBSTP = "FBSTP";
        public const string FCHS = "FCHS";
        public const string FCLEX = "FCLEX";
        public const string FCMOVcc = "FCMOVcc";
        public const string FCOM = "FCOM";
        public const string FCOMI = "FCOMI";
        public const string FCOMIP = "FCOMIP";
        public const string FCOMP = "FCOMP";
        public const string FCOMPP = "FCOMPP";
        public const string FCOS = "FCOS";
        public const string FDECSTP = "FDECSTP";
        public const string FDIV = "FDIV";
        public const string FDIVP = "FDIVP";
        public const string FDIVR = "FDIVR";
        public const string FDIVRP = "FDIVRP";
        public const string FFREE = "FFREE";
        public const string FIADD = "FIADD";
        public const string FICOM = "FICOM";
        public const string FICOMP = "FICOMP";
        public const string FIDIV = "FIDIV";
        public const string FIDIVRP = "FIDIVRP";
        public const string FILD = "FILD";
        public const string FINCSTP = "FINCSTP";
        public const string FINIT = "FINIT";
        public const string FIST = "FIST";
        public const string FISTP = "FISTP";
        public const string FISTTP = "FISTTP";
        public const string FLD = "FLD";
        public const string FLD1 = "FLD1";
        public const string FNCLEX = "FNCLEX";
        public const string FNINIT = "FNINIT";
        public const string FUCOMI = "FUCOMI";
        public const string FUCOMIP = "FUCOMIP";
        public const string HLT = "HLT";
        public const string HSUBPD = "HSUBPD";
        public const string HSUBPS = "HSUBPS";
        public const string IDIV = "IDIV";
        public const string IMUL = "IMUL";
        public const string IN = "IN";
        public const string INC = "INC";
        public const string INS = "INS";
        public const string INSERTPS = "INSERTPS";
        public const string INT = "INT";
        public const string INT3 = "INT3";
        public const string INTO = "INTO";
        public const string INVD = "INVD";
        public const string INVPCID = "INVPCID";
        public const string INVPG = "INVPG";
        public const string IRET = "IRET";
        public const string JCC = "JCC";
        public const string JMP = "JMP";
        public const string LAHF = "LAHF";
        public const string LAR = "LAR";
        public const string LDDQU = "LDDQU";
        public const string LDMXCSR = "LDMXCSR";
        public const string LDS = "LDS";
        public const string LEA = "LEA";
        public const string LEAVE = "LEAVE";
        public const string LES = "LES";
        public const string LFENCE = "LFENCE";
        public const string LFS = "LFS";
        public const string LGDT = "LGDT";
        public const string LGS = "LGS";
        public const string LIDT = "LIDT";
        public const string LLDT = "LLDT";
        public const string LMSW = "LMSW";
        public const string LODS = "LODS";
        public const string LOOP = "LOOP";
        public const string LOOPCC = "LOOPCC";
        public const string LSL = "LSL";
        public const string LSS = "LSS";
        public const string LTR = "LTR";
        public const string LZCNT = "LZCNT";
        public const string MASKMOVDQU = "MASKMOVDQU";
        public const string MASKMOVQ = "MASKMOVQ";
        public const string MAXPD = "MAXPD";
        public const string MAXPS = "MAXPS";
        public const string MAXSD = "MAXSD";
        public const string MAXSS = "MAXSS";
        public const string MFENCE = "MFENCE";
        public const string MINPD = "MINPD";
        public const string MINPS = "MINPS";
        public const string MINSD = "MINSD";
        public const string MINSS = "MINSS";
        public const string MONITOR = "MONITOR";
        public const string MOV = "MOV";
        public const string MOVAPD = "MOVAPD";
        public const string MOVAPS = "MOVAPS";
        public const string MOVBE = "MOVBE";
        public const string MOVD = "MOVD";
        public const string MOVDDUP = "MOVDDUP";
        public const string MOVDQ2Q = "MOVDQ2Q";
        public const string MOVDQA = "MOVDQA";
        public const string MOVDQU = "MOVDQU";
        public const string MOVHPS = "MOVHPS";
        public const string MOVLHPS = "MOVLHPS";
        public const string MOVLPD = "MOVLPD";
        public const string MOVLPS = "MOVLPS";
        public const string MOVMSKPD = "MOVMSKPD";
        public const string MOVMSKPS = "MOVMSKPS";
        public const string MOVNTDQ = "MOVNTDQ";
        public const string MOVNTDQA = "MOVNTDQA";
        public const string MOVNTI = "MOVNTI";
        public const string MOVNTPD = "MOVNTPD";
        public const string MOVNTPQ = "MOVNTPQ";
        public const string MOVNTPS = "MOVNTPS";
        public const string MOVSX = "MOVSX";
        public const string MOVQ = "MOVQ";
        public const string MOVZX = "MOVZX";
        public const string MUL = "MUL";
        public const string MULPD = "MULPD";
        public const string MULPS = "MULPS";
        public const string MULSD = "MULSD";
        public const string MULSS = "MULSS";
        public const string MULX = "MULX";
        public const string MWAIT = "MWAIT";
        public const string NEG = "NEG";
        public const string NOP = "NOP";
        public const string NOT = "NOT";
        public const string OR = "OR";
        public const string ORPD = "ORPD";
        public const string ORPS = "ORPS";
        public const string OUT = "OUT";
        public const string OUTS = "OUTS";
        public const string PABSB = "PABSB";
        public const string PACKSSWB = "PACKSSWB";
        public const string PAUSE = "PAUSE";
        public const string POP = "POP";
        public const string POPA = "POPA";
        public const string POPAD = "POPAD";
        public const string POPF = "POPF";
        public const string PUSH = "PUSH";
        public const string PUSHA = "PUSHA";
        public const string PUSHAD = "PUSHAD";
        public const string PUSHF = "PUSHF";
        public const string PUSHFD = "PUSHFD";
        public const string RCL = "RCL";
        public const string RCR = "RCR";
        public const string RDMSR = "RDMSR";
        public const string RDPKRU = "RDPKRU";
        public const string RDPMC = "RDPMC";
        public const string RDRAND = "RDRAND";
        public const string RDSEED = "RDSEED";
        public const string RDTSC = "RDTSC";
        public const string RDTSCP = "RDTSCP";
        public const string RET = "RET";
        public const string ROL = "ROL";
        public const string ROR = "ROR";
        public const string SAL = "SAL";
        public const string SAR = "SAR";
        public const string SBB = "SBB";
        public const string SHL = "SHL";
        public const string SHR = "SHR";
        public const string STAC = "STAC";
        public const string STC = "STC";
        public const string STD = "STD";
        public const string STI = "STI";
        public const string STOS = "STOS";
        public const string SUB = "SUB";
        public const string TEST = "TEST";
        public const string XCHG = "XCHG";
        public const string XOR = "XOR";
    }
}
