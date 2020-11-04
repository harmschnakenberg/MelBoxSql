using MelBox;

namespace MelBoxSql_Test
{
    class Program
    {
        static void Main()
        {
            MelSql sql = new MelSql();

            sql.Log(MelBox.MelSql.LogTopic.Start, MelSql.LogPrio.Info, "Dies ist der erste Manuelle Eintrag.");
        }
    }
}
