
namespace WarGame_True.Infrastructure.DebugPack {
    public abstract class Command {

        public CommandType commandType;

        public abstract void Excute();


    }

    public enum CommandType{
        ArmyCommand,
        ProvinceCommand,
        ResourceCommand
    }


}


//province population add 1000	    // ָ��ʡ���˿�����1000
//province population remove 1000
//province manpower add 1000
//province manpower remove 1000
//......

//faction money 1000			    // ��Ǯ
//faction manpower 1000             // ������
//......
//tag LKY             // תΪָ����tag
