using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;


public static class Utility
{
    public static AsyncOperation LoadCommonScene()
    {
        return SceneManager.LoadSceneAsync(Constant.COMMON_SCENE_NAME, LoadSceneMode.Additive);
    }

    public static WorldSettings GetWorldSettingsFromCommonScene()
    {
        // TODO: MORE ERROR CHECKING HERE!
        Scene commonScene = SceneManager.GetSceneByName(Constant.COMMON_SCENE_NAME);
        Debug.Log("Root scene count is: " + commonScene.rootCount);
        List<GameObject> rootGameObjects = new List<GameObject>();
        commonScene.GetRootGameObjects(rootGameObjects);
        // assume only one root object
        GameObject r = rootGameObjects[0];
        return r.GetComponent<WorldSettings>();
    }
}