using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HID_SIMPLE.HID
{
    //command types list
    public enum CommandType : byte
    {
        MotorActive = 0,
        MotorStop,
        MotorGoHome,
        NONCommand
    };

    public enum CommandState : byte
    {
        CommandState_normal = 0,
        CommandState_accidentlyTouchTop,
        CommandState_formateError,
        CommandState_parametersError,
        CommandState_busy

    };

    //command packaged struct
    public struct CommandStruct
    {
        public CommandType cmdType;
        public CommandState cmdState;
        public uint[] Params;
        public byte paramsLength;

        //public byte[] allBytes;
    };

    public class ClassCommandBytes
    {
        public const uint leastBytesCount = 7;  //the least bytes count
        const uint maxParamsCount = 10;
        const uint paramsLengthIndex = 3;  // the length of each parameters
        const uint paramsNumIndex = 4;  // the number of parameters
        const uint paramsIndex = 5;  // the parameters start
        const byte cmdStartFlag = 0xFA;
        const byte cmdEndFlag = 0xA5;
        const byte reponseStartFlag = 0xFB;
        const byte reponseEndFlag = 0xA6;


        public CommandStruct UnPackageBytes(byte[] byteCMD)
        {
            CommandStruct cs = new CommandStruct();
            //cs.allBytes = byteCMD;

            cs.cmdState = CommandState.CommandState_formateError;

            // reponse start
            if (byteCMD[0] != reponseStartFlag)
                return cs;


            //type
            cs.cmdType = (CommandType)byteCMD[2];

            //pL pN
            uint paramsLength = byteCMD[paramsLengthIndex];  //default is 4
            uint paramsNum = byteCMD[paramsNumIndex];
            cs.Params = new uint[byteCMD.Length];

            //state param1
            byte[] state = new byte[paramsLength];
            for (int i = 0; i < paramsLength; i++)
            {
                state[i] = byteCMD[paramsIndex + i];
            }


            if (CompareArray(state , new byte[] { 0x00, 0x00, 0x00, 0x00 }) ) //CommandState_normal = 0
            {
                cs.cmdState = CommandState.CommandState_normal;
            }
            else if (CompareArray(state ,  new byte[] { 0x00, 0x00, 0x00, 0x01 })) //CommandState_accidentlyTouchTop
            {
                cs.cmdState = CommandState.CommandState_accidentlyTouchTop;
            }

            else if(CompareArray(state ,  new byte[] { 0xff, 0xff, 0xff, 0xff })) //CommandState_formateError
            {
                cs.cmdState = CommandState.CommandState_formateError;
            }
            else if(CompareArray(state ,  new byte[] { 0xff, 0xff, 0xff, 0xfe })) //CommandState_parametersError
            {
                cs.cmdState = CommandState.CommandState_parametersError;
            }
            else if(CompareArray(state ,  new byte[] { 0xff, 0xff, 0xff, 0xfd })) //CommandState_busy
            {
                cs.cmdState = CommandState.CommandState_busy;
            }
            else  // command error
            {
                return cs;
            }
 

            //parameters
            uint t;
            for (t = 0; t < paramsNum-1; t++)
            {
                uint param = 0;

                for (int j = 0; j < paramsLength; j++)
                {
                    // skip state
                    param += Convert.ToUInt32(byteCMD[paramsIndex +4 + paramsLength * t + j] * Math.Pow(256, j));
                }

                cs.Params[t] = param;

            }
            cs.paramsLength = (byte)(t + 1);

            //checksum


            //end
            if (byteCMD[byteCMD.Length - 1] != reponseEndFlag)
                cs.cmdState = CommandState.CommandState_formateError;

            return cs;
        }

        public byte[] PackageCMD(CommandStruct cmdStruct)
        { 

            uint length = Convert.ToUInt32(7 + cmdStruct.paramsLength*4);  //start type2 pL pN state4 parameters4*num checksum end
    
            byte[]  SendBytes = new byte[length];
    
            uint  j=0;
    
            //start
            SendBytes[j] = cmdStartFlag;
            j++;
    
            //type
            SendBytes[j] = 0x00;
            j++;
            SendBytes[j] = (byte)cmdStruct.cmdType;
            j++;
    
            //pL pN
            SendBytes[j] = 0x04;
            j++;
            SendBytes[j] = cmdStruct.paramsLength;
            j++;
    

            uint   i=0;
            //parameters
            for (i = 0; i < cmdStruct.paramsLength; i++)
            {
                int  t = 0;
                for (t = 0; t < 4; t++)
                {
                    int parmTmp = Convert.ToInt32(cmdStruct.Params[i]);
                    //little enbian
                    SendBytes[j] = Convert.ToByte((parmTmp >> (8 * t) )  %  256) ;
                    j++;
                }

            }
    
            //package the checksum
            SendBytes[j] = getCheckSum(SendBytes);
            j++;
    
            //package the end
            SendBytes[j] = cmdEndFlag;

            return SendBytes;
        }

        //check the checkSum
        bool CheckSumCheck(byte[] byteCMD)
        {
            byte cmdCheckSum = byteCMD[byteCMD.Length- 2];

            byte realCheckSum = getCheckSum(byteCMD);
	
	        if(cmdCheckSum == realCheckSum)
		        return true;
	        else
		        return false;
        }

        //get the checksum
        byte  getCheckSum(byte[]  byteCMD)
        {
	        byte  realCheckSum = 0;
	
	        uint  j;
            for (j = paramsLengthIndex; j < byteCMD.Length - 2; j++)
	        {
		        realCheckSum ^= byteCMD[j];
	        }
            return realCheckSum;
        }

        bool CompareArray(byte[] a, byte[] b)
        {
            for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }
    }
}
