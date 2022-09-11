

using System.Threading.Tasks;
using UnityEngine;

namespace Jyx2
{
    public static class BeforeSceneLoad
    {
        public static void ColdBind()
        {
            DebugInfoManager.Init();

            loadFinishTask = StartTasks();
        }

        static async Task StartTasks()
        {
            GameSettingManager.Init();
            await Jyx2ResourceHelper.Init();
        }

        public static Task loadFinishTask = null;
    }
}
