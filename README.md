##############################
# 运行软件之前应先安装运行库（就是叫做“dotnet-sdk-6.0.412-win-x64”的程序，下载好后双击运行）
##############################
## 本软件有这样两个功能：
  1.通过bat文件启动在Webs.txt中记录的网址或者是本地路径；
  2.分门别类地抽取CorrectNoteBook文件夹中的错题
## 使用方法：
  1.如果你想启动网址，请打开webs文件夹下的Webs.txt文件，按照以下格式编写（格式就是你跟他起的名字然后一个英文分号然后写上完整的网址，就是http开头的）：你给他起的名字;网址
  2.如果想启动本地文件夹（路径）先在Transport文件加下创建一个快捷方式，快捷方式的名字不能有中文或者空格，快捷方式指向的路径更改为你想启动的本地文件夹的路径，然后把这个快捷方式的路径按照“使用方法1”那样写好即可；
  3.如果想复习错题，就在CorrectNoteBook中放好图片，或者也可以在里面新建子文件夹然后再放，在程序中你可以选择从全部的图片中随机抽（就是在第一次提问“是否按照分类抽取？”的时候选择“n”），也可以逐级地选择（就是在提问“是否按照分类抽取？”的时    候选择“y”），一直到你想想要的文件夹（当你在选择“n”后，并且程序随后列出的文件夹名单中看见了你想抽取的文件夹的名字，那么在这一次问你的时候，请输入“y”）
