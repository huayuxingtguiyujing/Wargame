# Wargame

各位好，这是一个12年p社老玩家也是一个入门程序员的游戏，暂名wargame。游戏的背景发生在残唐五代，即公元755年安史之乱爆发，到975年宋朝统一中国的这段历史。在这段鲜为人知的历史中，充斥着尔虞我诈，纵横捭阖，起义、叛乱和颠覆。玩家可以在指定的剧本中选择势力，你可以选择扮演安史叛军，在叛乱中摧毁唐王朝；也可以选择扮演后唐，击败朱梁统一天下再造大唐；更可以选择其他出其不意的候选人，逐鹿中原。总之，你需要经营经济，灵活外交，指挥军队，摧毁所有敌对势力，最后让你的势力登顶。

（好了，以上都还是饼！目前版本不一定有！包装不代表实物！）该游戏是一个使用unity制作的战棋游戏，仿自欧陆风云、欧陆战争等策略类游戏，目前还只是一个半成品，估算距完整项目的完成度为30%，美术和音频资源均来自于公开网站。
在本文件当中，会演示项目的当前内容，并介绍该项目的代码技术框架。

# 一.当前内容

1.开始界面
游戏支持单机游玩和多人模式。多人联网基于unity netcode实现。
![e0e29855e0d18300ece5884caff0d10](https://github.com/huayuxingtguiyujing/Wargame/assets/116824077/d072d70d-ad12-4952-8398-f85458b283dc)
使用多人游玩时，可以选择建立房间或者直连，分别使用unity relay和lobby包实现。游戏暂不支持设置调节等功能。
![590d7ed629e98a4f646629a5e2a4a3d](https://github.com/huayuxingtguiyujing/Wargame/assets/116824077/1f571a96-844f-479b-9ec8-bfb27d61da82)

2.选择势力界面：
在游戏中，你需要选择一个指定的剧本扮演其中一个势力，目前仅提供了“五代十国”剧本中的部分势力，其他剧本和势力会在后续更新。
![510e1a581580f0384e9c62383255ed5](https://github.com/huayuxingtguiyujing/Wargame/assets/116824077/21566433-9a35-42be-a028-2ebc52058097)

3.游戏界面
通过WASD移动镜头，滚轮进行缩放，点击省份展开省份界面，在省份界面可以招募军队，框选军队以查看军队属性。
![252105baa770f5d33bb74f66bf9e9e5](https://github.com/huayuxingtguiyujing/Wargame/assets/116824077/e6bec3ac-7a6c-4c63-a998-f6661f42f21f)
在省份界面可以建立建筑。左上角显示了当前势力的各项资源，点击各个选项可以打开经济、粮食、外交等面板，以查看收支明细。点击右上角的时间ui以控制游戏时间运行。
![9d8fff0480a12d760f8b965481ad892](https://github.com/huayuxingtguiyujing/Wargame/assets/116824077/bf245497-6f72-49ce-9d97-4734890a3943)
框选军队点击右键可以移动，军队到达敌方省份会自动开始占领省份，敌我双方军队会自动进行战斗
![36dd70d5f7fa679a1d0a65d98639893](https://github.com/huayuxingtguiyujing/Wargame/assets/116824077/4ae26aae-165a-4e1e-82c1-c69558c6164c)
![d853c0cbb79451f0b4ae44312f90309](https://github.com/huayuxingtguiyujing/Wargame/assets/116824077/52b357ad-185b-4a50-8b69-dd28a316eff5)

游戏目前还没有ai......挺多功能还未完善......（dbq我是懒狗  o(TヘTo) ）

# 二.项目代码框架
游戏的代码均位于Assets/Scripts文件夹下，该文件夹下主要有三块内容

1.GamePlay文件夹 

游戏逻辑强相关的功能均在此文件夹，此文件夹下的主要内容有：

(1).时间管理：封装了游戏的时间数据，提供了一个时间管理器和ui来让玩家可以在游戏里管理时间，暂停继续或者加减速游戏内时间。使用订阅模式为程序的其他部分挂载游戏的时间结算事件

(2).军队模块：基于mvc实现军队的数据、管理与ui视图分离解耦，提供了一个军队管理器实现对军队单位的创建、合并、拆分、销毁、移动等功能

(3).战斗模块：将战斗事务封装为一个随时间流逝而进行不断结算的事件，实现了军队在战斗中的攻击、伤亡、buff加成功能，基于状态机实现了战斗中不同阶段的转换（对峙-接战-合战-撤退）

(4).势力模块：封装了国家势力类，向外提供使用势力资源的接口

(5).UI模块：包括一个ui管理框架，使用ui栈在游戏内动态加载ui资源


2.Infrastructure文件夹 

自己写的可复用的工具包，该包的内容均为自己封装的一些小工具，为了方便重用和扩展，把一些游戏内的基础实现放在了此包下，此文件夹下的主要内容有：

(1).六边形网格地图系统：作为游戏地图的基础，实现了六边形运算，坐标系统，省份数据类，基于csv和excel的地图数据持久化功能等。该包还提供一个网格地图生成器，支持通过inspector界面进行地图修改

(2).联网系统：基于unity netcode + relay + Lobby + Authtication + Mirror 构建的联网系统，实现了多人主机直连建立房间进行联网对战的功能。基于状态机实现了从离线到主机到客户端的不同联网状态的转换。该部分参考了官方提供的bossroom示例：[unity的bossroom示例](https://github.com/Unity-Technologies/com.unity.multiplayer.samples.coop.git)

(3).策略战棋ai系统：还没写呢 o(TヘTo)

(4).其他：音频管理、ui管理框架、场景管理器、对象池等常用的组件


3.UnityService文件夹

包括了对unityservice功能的封装，以更方便地使用它们
