using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// 使い方　「remove_in_bulk_all_missing_error_script.cs」をダウンロードするかソースから作成する。
// １：プロジェクト(Assets)に追加で自動認識し、Unityエディタのメニューバーに「exception_handling」が追加。
// ２：ヒエラルキーから階層構造トップの▶をALTを押したままクリックすると直下子オブジェクトが全展開。
// ３：全展開されたオブジェクトを範囲選択する(先頭クリックしSHIFTを押したまま末尾をクリック)。
// ４：Unityエディタのメニューバー「exception_handling」の中の「all_missing_error_script_removal_in_bulk」を選択。
// ５：全展開の全選択されたオブジェクトからMissingScriptを削除し、削除数をDebug.Logに表示。
// ６：ざっくりと確認。プロジェクトを保存。「remove_in_bulk_all_missing_error_script.cs」を削除する。

public class exception_handling
{
    [MenuItem("exception_handling/all missing error script removal in bulk")]
    private static void all_missing_error_script_removal_in_bulk()
    {
        int Sgo = 0;
        int i;
        int missingCount = 0;
        Sgo = Selection.gameObjects.Length;
        Debug.Log("SelectObj = " + Sgo);

        GameObject[] target_obj;
        if(Sgo >= 1){
            target_obj = Selection.gameObjects;
            for(i=0; i<Sgo; i++){
                missingCount += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(target_obj[i]);
            }
        }
        Debug.Log("missingCountObj = " + missingCount);
    }

}