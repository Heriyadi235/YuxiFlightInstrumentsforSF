# YuxiFlightInstrumentsforSF

## 简介
一套飞行仪表系统，用于基于[SaccFlight](https://github.com/Sacchan-VRC/SaccFlightAndVehicles)的VRChat飞行地图。
目前主要做完了空速表、姿态仪、高度表、转弯侧滑仪与升降计(六大金刚)。

[一个示意图](https://github.com/Heriyadi235/YuxiFlightInstrumentsforSF/blob/main/documents/pic1.png)
这里要插入个图片

虽然乍一看仪表模型有点简陋(谁让俺不会建模捏)，但是编写脚本时候有考虑到复用性，所以应该可以方便地将其替换为自己的仪表模型，只需设置好动画，无需编写额外的代码。

## 繁介
咱就是说有一天突然突然想自己做一个SF的机模，翻了翻SaccFlight的代码，然后就突然觉得，UdonSharp也不咋难嘛，花了两三天的时间，补了点开发知识，搞个这么个玩意。
总体来说，让仪表动起来需要2个脚本，1个动画控制器，程序里发生事情基本上就是下面这样：
* 脚本
[YFI_FlightDataInterface.cs](https://github.com/Heriyadi235/YuxiFlightInstrumentsforSF/blob/main/Scripts/YFI_FlightDataInterface.cs)
用于从SAVController中读取或计算大部分飞行数据。
* 脚本
[YFI_AnimatorController.cs](https://github.com/Heriyadi235/YuxiFlightInstrumentsforSF/blob/main/Scripts/YFI_AnimatorController.cs)
用于将飞行数组转换为0~1的参数以驱动仪表所属的动画控制器，可以在这里设置仪表量程等参数

不久之后我会更新一下上述过程的更多细节捏~

## Intruduce(ENG)
A flight instrusments script for flight world in VRChat with [SaccFlight](https://github.com/Sacchan-VRC/SaccFlightAndVehicles) .
Including Basic-T(Altimeter, Airspeed indicator, Vertical speed indicator, Attitude Indicator,Heading indicator and Turn indicator)
Example indicators come with repositories may look not that exquisitem(sorry for my poor modeling skill), but it's easy to substitute them with your own indicator models.
To use another model, set the animator controller as require, set the params (SAVcontroller, animator controller, measurement range of a certain indicator etc.) for 
[YFI_FlightDataInterface.cs](https://github.com/Heriyadi235/YuxiFlightInstrumentsforSF/blob/main/Scripts/YFI_FlightDataInterface.cs) and
[YFI_AnimatorController.cs](https://github.com/Heriyadi235/YuxiFlightInstrumentsforSF/blob/main/Scripts/YFI_AnimatorController.cs) should be enough, no added code required.

An instrument with more detail will be updated soon.


