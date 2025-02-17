# WarGameDemo

#### 这是什么？：

这是一个用于验证游戏制作思路的`demo`，是一个使用`unity`制作的战棋游戏，等待后续的更新维护。<br>
该`demo`仿自`欧陆风云、欧陆战争`等策略类游戏，目前是一个半成品，立绘、背景图和音频资源均来自于公开网站。<br>

#### 有什么功能？：
-地图：六边形格子作为基本单位的地图，规模`30000`，使用中国地图作为背景；<br>
-军队：创建移动消耗军队，军队之间的[战斗](https://github.com/huayuxingtguiyujing/Wargame/tree/main/Assets/Script/GamePlay/Combat)，还有围城机制；<br>
-AI：`行为树+势力图`方案构建的策略游戏AI；<br>
-联网：用`unity`官方推荐的`Netcode`工作流构建的联网模块，支持局域网连接；<br>

#### 游戏内容：

游戏支持单机游玩和多人模式，主菜单界面如下;<br>
![e0e29855e0d18300ece5884caff0d10](https://github.com/huayuxingtguiyujing/Wargame/assets/116824077/d072d70d-ad12-4952-8398-f85458b283dc)
使用多人游玩时，可以选择建立房间或者直连，直连目前仅支持局域网连接：
![590d7ed629e98a4f646629a5e2a4a3d](https://github.com/huayuxingtguiyujing/Wargame/assets/116824077/1f571a96-844f-479b-9ec8-bfb27d61da82)
在游戏中，你需要选择一个指定的剧本扮演其中一个势力;<br>
![510e1a581580f0384e9c62383255ed5](https://github.com/huayuxingtguiyujing/Wargame/assets/116824077/21566433-9a35-42be-a028-2ebc52058097)
通过WASD移动镜头，滚轮进行缩放，点击省份展开省份界面，在省份界面可以招募军队，框选军队以查看军队属性;<br>
![252105baa770f5d33bb74f66bf9e9e5](https://github.com/huayuxingtguiyujing/Wargame/assets/116824077/e6bec3ac-7a6c-4c63-a998-f6661f42f21f)
左上角显示了当前势力的各项资源，点击各个选项可以打开经济、粮食、外交等面板，以查看收支明细。点击右上角的时间ui以控制游戏时间运行，可以减少/加快游戏时间流逝速度、暂停时间。
![9d8fff0480a12d760f8b965481ad892](https://github.com/huayuxingtguiyujing/Wargame/assets/116824077/bf245497-6f72-49ce-9d97-4734890a3943)
框选军队点击右键可以移动，军队到达敌方省份会自动开始占领省份，敌我双方军队会自动进行战斗;<br>
![36dd70d5f7fa679a1d0a65d98639893](https://github.com/huayuxingtguiyujing/Wargame/assets/116824077/4ae26aae-165a-4e1e-82c1-c69558c6164c)
![d853c0cbb79451f0b4ae44312f90309](https://github.com/huayuxingtguiyujing/Wargame/assets/116824077/52b357ad-185b-4a50-8b69-dd28a316eff5)

#### 相关链接：

`unity`官方相关：<br>
[`unity`的`bossroom`示例，演示 `Netcode` 的使用](https://github.com/Unity-Technologies/com.unity.multiplayer.samples.coop.git);<br>
[`unity cloud` 官网](https://cloud.unity.com/);<br>
[`unity game service` 官网](https://unity.com/cn/solutions/gaming-services);<br>

地图部分受下面的博客启发：<br>
[catlike-hex map](https://catlikecoding.com/unity/tutorials/hex-map/); <br>
[hexagons grids](https://www.redblobgames.com/grids/hexagons/);<br>
伟大，无需多言！

