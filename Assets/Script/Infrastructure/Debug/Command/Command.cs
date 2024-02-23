
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


//province population add 1000	    // 指定省份人口增加1000
//province population remove 1000
//province manpower add 1000
//province manpower remove 1000
//......

//faction money 1000			    // 加钱
//faction manpower 1000             // 加人力
//......
//tag LKY             // 转为指定的tag
