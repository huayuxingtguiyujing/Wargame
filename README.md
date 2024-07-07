# Wargame

各位好，这是一个12年p社老玩家的游戏，暂名wargame。游戏的背景发生在残唐五代，即公元755年安史之乱爆发，到975年宋朝统一中国的这段历史。在这段鲜为人知的历史中，充斥着尔虞我诈，纵横捭阖，起义、叛乱和颠覆。玩家可以在指定的剧本中选择势力，你可以选择扮演安史叛军，在叛乱中摧毁唐王朝；也可以选择扮演后唐，击败朱梁统一天下再造大唐；更可以选择其他出其不意的候选人，逐鹿中原。总之，你需要经营经济，灵活外交，指挥军队，摧毁所有敌对势力，最后让你的势力登顶。

该游戏是一个使用unity制作的战棋游戏，仿自欧陆风云、欧陆战争等策略类游戏，目前还是一个半成品，估算距完整项目的完成度为30%。立绘、背景图和音频资源均来自于公开网站，部分UI元素由自己绘制（PS、AI）。

在本文件当中，会演示项目的当前内容，并介绍该项目的代码技术框架。

# 一.游戏内容介绍

1.开始界面
游戏支持单机游玩和多人模式。多人联网基于unity netcode实现，主菜单界面如下。
![e0e29855e0d18300ece5884caff0d10](https://github.com/huayuxingtguiyujing/Wargame/assets/116824077/d072d70d-ad12-4952-8398-f85458b283dc)
使用多人游玩时，可以选择建立房间或者直连，分别使用unity relay和lobby包实现，直连目前仅支持局域网连接。游戏暂不支持设置调节等功能。
![590d7ed629e98a4f646629a5e2a4a3d](https://github.com/huayuxingtguiyujing/Wargame/assets/116824077/1f571a96-844f-479b-9ec8-bfb27d61da82)

2.选择势力界面：
在游戏中，你需要选择一个指定的剧本扮演其中一个势力，目前仅提供了“五代十国”剧本中的部分势力，其他剧本和势力会在后续更新。
![510e1a581580f0384e9c62383255ed5](https://github.com/huayuxingtguiyujing/Wargame/assets/116824077/21566433-9a35-42be-a028-2ebc52058097)

3.游戏界面
通过WASD移动镜头，滚轮进行缩放，点击省份展开省份界面，在省份界面可以招募军队，框选军队以查看军队属性。
![252105baa770f5d33bb74f66bf9e9e5](https://github.com/huayuxingtguiyujing/Wargame/assets/116824077/e6bec3ac-7a6c-4c63-a998-f6661f42f21f)
在省份界面可以建立建筑。左上角显示了当前势力的各项资源，点击各个选项可以打开经济、粮食、外交等面板，以查看收支明细。点击右上角的时间ui以控制游戏时间运行，可以减少/加快游戏时间流逝速度、暂停时间。
![9d8fff0480a12d760f8b965481ad892](https://github.com/huayuxingtguiyujing/Wargame/assets/116824077/bf245497-6f72-49ce-9d97-4734890a3943)
框选军队点击右键可以移动，军队到达敌方省份会自动开始占领省份，敌我双方军队会自动进行战斗
![36dd70d5f7fa679a1d0a65d98639893](https://github.com/huayuxingtguiyujing/Wargame/assets/116824077/4ae26aae-165a-4e1e-82c1-c69558c6164c)
![d853c0cbb79451f0b4ae44312f90309](https://github.com/huayuxingtguiyujing/Wargame/assets/116824077/52b357ad-185b-4a50-8b69-dd28a316eff5)

4.游戏ai
ai使用行为树+势力图实现，参考欧陆风云ai开发日志。可以通过右下角的地图模式，切换到ai权重分布图，显示ai在每个单元格上的防御、攻击权重；

# 二.项目代码框架

## [GamePlay](https://github.com/huayuxingtguiyujing/Wargame/tree/main/Assets/Script/GamePlay)

游戏逻辑强相关的功能均在此文件夹，此文件夹下的主要内容有：

1.[时间管理](https://github.com/huayuxingtguiyujing/Wargame/tree/main/Assets/Script/GamePlay/Application/Time)：<br>
封装了游戏的时间数据，提供了一个时间管理器和ui来让玩家可以在游戏里管理时间，暂停继续或者加减速游戏内时间。游戏中的所有逻辑更新，比如军队移动、税收、事件触发等均依赖该时间系统。该系统使用观察者模式为程序的其他部分挂载游戏的时间结算事件，目前有四类结算事件：小时结算、日结算、月结算、年结算，每当游戏时间运行了上述间隔后就会触发一次更新。该包下封装了TimeTask时间事务，作为大部分时间结算事件的基类，该类事务执行时会按指定间隔执行逻辑，执行到期时可以触发事件对应效果，目前已经应用到大部分游戏逻辑中，如招募军队、建造建筑、军队移动、军队战斗等。

2.[军队模块](https://github.com/huayuxingtguiyujing/Wargame/tree/main/Assets/Script/GamePlay/Army)：<br>
基于mvc实现军队的数据、管理与ui视图分离解耦，提供了一个军队管理器实现对军队单位的创建、合并、拆分、销毁、移动等功能，玩家可以通过省份界面招募军队，执行一个时间结算事件，结算完毕后在当地生成军队单位。点击地图网格以移动军队，军队的移动使用A*算法实现，其实就是简单的迪杰斯特拉/BFS作为一般项 + 曼哈顿距离/切比雪夫距离作为启发项。当军队到达了一个敌对的，不被自己占领的省份时，会触发一个省份围城事件，围城进度会根据守城方的军队（如果有）和攻城方的军队进行推进，目前未实际完成围城事件的同步。军队的战斗相关见下文，涉及到比较复杂的游戏逻辑。

联网模式下，通过networkobject的id实现各个加入的主机的军队单位同步，包括位置同步、状态同步，该id是一个网络同步变量，详细可见netcode for gameobject包，该包还有一个支持ESC的版本：netcode for entity。

3.[战斗模块](https://github.com/huayuxingtguiyujing/Wargame/tree/main/Assets/Script/GamePlay/Combat)：<br>
战斗事务是一个随时间流逝而进行结算的事件，该模块实现了军队在战斗中的攻击、伤亡、buff加成功能。基于状态机实现了战斗中不同阶段的转换（对峙-接战-合战-撤退），对峙阶段持续最久，是对现实战役中两军开战前僵持消耗的模拟，此阶段双方伤亡均有限，接站和合战阶段时军队战斗烈度加大。军队撤退时，可以选择撤退到的目标省份，如果是被击败而触发撤退事件，则会自动搜索附近的安全省份并移动到。

游戏内的将领系统也包括在此模块内，每名将领均在游戏内通过一个scriptableobject进行存储，允许通过csv文本批量生成与配置将领，也可以将将领信息输出到csv文件中。除开对军队士气组织度的属性提升外，将领在战役的不同阶段均有不同的加成，也有不同的技能（待完成）。每只军队仅能有一位将领

4.[势力模块](https://github.com/huayuxingtguiyujing/Wargame/tree/main/Assets/Script/GamePlay/Politic)：<br>
封装了国家势力类，向外提供操作势力资源的接口，税收、粮食等资源的结算均在此进行

5.[UI模块](https://github.com/huayuxingtguiyujing/Wargame/tree/main/Assets/Script/GamePlay/UI)：<br>
包括一个ui管理框架，使用ui栈管理当前场景的ui面板，可在游戏内动态加载ui资源


## [Infrastructure](https://github.com/huayuxingtguiyujing/Wargame/tree/main/Assets/Script/Infrastructure)

自己写的可复用的工具包，该包的内容均为自己写的一些小工具，为了方便重用和扩展，把一些游戏内的基础实现拆分出了此包，此文件夹下的主要内容有：

1.[六边形网格地图系统](https://github.com/huayuxingtguiyujing/Wargame/tree/main/Assets/Script/Infrastructure/HexagonalGrids)：<br>
作为游戏地图的基础，是一个仿文明6、三国志11的地图系统，基于MeshRender实现，目前该包有六边形单元生成、单元高程差、随机扰动、河流生成、水模拟、地图格纹理等功能，并未完全完成。基于csv和excel完成了地图的数据持久化。该包还提供一个网格地图生成器，支持通过inspector界面对地图进行快捷修改。<br>
在各种策略游戏的地图生成算法中，有文明6的洪泛算法，文明4的砖石-棱形算法，欧陆风云的沃诺洛伊图，本项目采用最简单方便的扫描线算法生成地图。地图数据使用MapController和HexConstructor两个控制器类存储，每个地图单元都有一个Province和HexGrid数据类，游戏中可以通过控制器访问省份单元的数据<br>
地图信息通过自己编写的一个基于CSV文本的存储模块进行存储，不考虑使用json等格式，window系统中可以通过C盘下的unity持久化存储路径，对预设定的省份信息直接进行更改，省份信息格式可参考该csv文件<br>
水和河流部分均使用shader编程实现，目前该块瑕疵较多，且有不自然的过渡，正在思考解决方案<br>
该部分受以下两个开源项目影响较大：<br>
[catlike-hex map](https://catlikecoding.com/unity/tutorials/hex-map/); <br>
[hexagons grids](https://www.redblobgames.com/grids/hexagons/);<br>
十分感谢以上项目的作者，等以后有余资，我会上patron赞助你们的！

2.[联网系统](https://github.com/huayuxingtguiyujing/Wargame/tree/main/Assets/Script/Infrastructure/Network)：<br>
基于unity netcode + relay + Lobby + Authtication + Mirror 构建的联网系统，实现了多人主机直连建立房间进行联网对战的功能，提供了游戏逻辑同步、网络连接、联网用户管理、离线重连等功能。基于状态机实现了从离线到主机到客户端的不同联网状态的转换。该部分参考了官方提供的bossroom示例：<br>
[unity的bossroom示例](https://github.com/Unity-Technologies/com.unity.multiplayer.samples.coop.git)

3.策略战棋ai系统：<br>
行为树+势力图，行为树包自己编写（可视化的behaviour tree编辑器包太复杂，且自己写写底层的东西挺有帮助），行为树的大部分基本节点均有实现，如各种复合节点。势力图算法需结合网格地图系统，原理较简单：设置权重来源 - 执行传播算法 - 更新权重值。设计的ai判断比较复杂，且仅适合于策略战棋类的游戏，不具有普适性，故不写。

4.其他：<br>音频管理、ui管理框架、场景管理器、对象池等常用的组件，有些是自己以前写些小项目时用上的，现搬运到此。


## UnityService

包括了对UGS功能的封装，以更方便地使用它们。涉及到的功能包可参阅：<br>[unity cloud 官网](https://cloud.unity.com/); <br>[unity game service 官网](https://unity.com/cn/solutions/gaming-services);
