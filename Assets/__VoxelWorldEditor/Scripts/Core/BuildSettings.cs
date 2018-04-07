﻿//
// Author  : Oliver Brodhage
// Company : Decentralised Team of Developers
//

namespace DragginzVoxelWorldEditor
{
    public static class BuildSettings
    {
		public static readonly string VWE_UnityClientScene  = "vwe_main";
		public static readonly string VWE_SplashScreenScene = "vwe_splash";

        public static readonly string UnityClientScene  = "main";
        public static readonly string SplashScreenScene = "splash";
        
		public static readonly string ClientDefaultActiveScene = UnityClientScene;
        public static readonly string[] ClientScenes = { UnityClientScene, SplashScreenScene };

        public static readonly string UnityWorkerScene = "UnityWorker";
        public static readonly string WorkerDefaultActiveScene = UnityWorkerScene;
        public static readonly string[] WorkerScenes = { UnityWorkerScene };

        public const string SceneDirectory = "Assets";
    }
}