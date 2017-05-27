极速验证码自动验证

思路
先截取两张图，一张是背景图，另一张是滑动滑块后，做法是把滑块滑到最左边的位置。
鉴于滑块有凹入和凸出的情况,主要是因为凸出的情况，因为凸出会增加整个滑块的长度，滑块的长度大概是45，目前定义的长度是60。
从X坐标60开始从左往右边找，算出点(X,Y)的RGB，对比两张表的RGB差值，目前定义的容忍度是100，如果超过容忍值就返回X坐标,运行结果发现可以在一定程度上跳过干扰。
如果滑块和阴影重合，就从右往左找，找到X以后减去滑块的长度45就是移动的距离。
关于滑块滑动X,Y,Time的算法，网络上说的匀速算法并不能逃过机器识别，目前处理的方法是加速拉。


验证码加入了5次重试机制。如果判断为非人为滑动，则刷新验证码，重新得到背景图和滑动把滑块放到最左边的图片，计算滑动轨迹。否则，重新计算滑动轨迹重试。


处理思路见
https://charles427.github.io/blog/%E6%BB%91%E5%9D%97%E9%AA%8C%E8%AF%81%E7%A0%81%E7%A0%B4%E8%A7%A3.html
