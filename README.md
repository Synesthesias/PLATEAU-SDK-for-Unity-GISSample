# PLATEAU SDK for Unity GIS Sample

![](./ReadmeImages/ScreenShotDay.png)

## 概要
[PLATEAU SDK for Unity](https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity) および [PLATEAU SDK Toolkits for Unity](https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity) のサンプルです。  
次の機能をデモンストレーションします。  
- PLATEAU SDK for Unityを利用し、PLATEAUの属性情報を可視化します。
  - このサンプルでは、SDKで読み取った属性情報の文字表示、また色分けによる視覚化を行います。
- PLATEAU SDK Toolkits for Unity (Map Toolkit) のGIS読込機能を利用し、国土数値情報を可視化します。
  - このサンプルでは、宙に浮かぶ文字等により路線名、公園名、学校名とその位置を表示します。
- PLATEAU SDK Toolkits for Unity (Rendering Toolkit) の機能を利用し、建物の見た目を向上します。
  - このサンプルでは、同機能により時間(昼と夜)や天候を切り替えできます。
  - 夜に設定すると、建物の窓などが光り、夜景を再現します。
- 遠景の景観表示機能はCesiumによるものです。

![](./ReadmeImages/ScreenShotNight.png)

## サンプルの利用方法
### サンプルの開き方
- Unityプロジェクトをダウンロードし、Unity 2021.3.30f1 で開きます
- シーンファイル `Assets/GISSample/GISSampleScene`を開き、再生します。

### 操作方法と画面の説明
- 画面左では情報の可視化を設定できます。
  - 「フィルタリング」では、高さやLODが指定の範囲内にある建物のみを表示し、他を非表示にできます。
  - 「天候」では時間、雨、雪、雲を設定できます。
  - 「色分け」ではPLATEAUの属性情報を色分けで可視化します。
    - 「高さ」は属性情報から建物の高さを取得して建物を色分けします。
    - 「建築物浸水リスク」は浸水想定高に応じて建物を色分けします。
    - 「想定浸水区域図」では、浸水想定区域を表示して色分けします。
    - なお建物の色分けは夜では見ずらいため、昼に設定することを推奨します。
- 画面右には、クリックした地物のPLATEAU属性情報が表示されます。
- 宙に浮かぶ文字ウィンドウは、国土数値情報を可視化したものです。
- カメラは移動可能です。

## サンプルの仕組み
- 近景には属性情報を含むPLATEAU都市モデルがあります。  
  PLATEAUの属性情報はシーンにコンポーネントとして配置され、PLATEAU SDKによって読みこまれることで  
  クリックでの属性表示や色分けを実装しています。
- 時間や天候の変更機能は、PLATEAU SDK Toolkits for Unityの機能の1つである、  
  Rendering Toolkitの「環境システムの設定」によるものです。
- 夜にすると建物の窓等が光り輝く機能は、Rendering Toolkitの「自動テクスチャ生成」によるものです。
- 空中に浮かぶ文字で路線名、公園名、学校名が表示されるのは、国土数値情報を元にしています。  
  国土数値情報などGISデータは、Toolkitsの機能の1つであるMaps Toolkitの「GISデータ読み込み」によるものです。
- 遠景の表示にはCesiumを利用しています。
  
## ライセンス
- 本リポジトリはMITライセンスで提供されています。
- 本システムの開発は株式会社シナスタジアが行っています。
- ソースコードおよび関連ドキュメントの著作権は国土交通省に帰属します。
- 利用ライブラリおよびデータについてはThirdPartyNotices.mdに記載しています。

## 注意事項
- 本リポジトリの内容は予告なく変更・削除する可能性があります。
- 本リポジトリの利用により生じた損失及び損害等について、国土交通省はいかなる責任も負わないものとします。
