> 本文服务器基于centos7，客户端Windows10

## FTP

FTP(File Transfer Protocol)，文件传输协议，是一个比较古老的基于TCP，用于不同计算机间传递文件的协议。

#### 安装

```shell
# 判断是否安装过ftp服务或客户端
yum list installed | grep ftp

# vsftpd是服务端，ftp是客户端
yum install vsftpd ftp;
```

ftp配置文件位于 **/etc/vsftpd** 目录下，/etc/vsftpd目录结构如下：

/etc/vsftpd  
|-- chroot_list  
|-- ftpusers  
|-- user_list  
|-- vsftpd.conf  
`-- vsftpd_conf_migrate.sh

其中，vsftpd.conf是主配置文件；ftpusers是黑名单，该文件中的用户不允许登录系统；user_list是黑白名单，根据vsftpd.conf中的配置决定user_list是白名单还是黑名单；chroot_list，根据vsftpd.conf中的配置决定该文件中的用户在登录时是否执行chroot操作。

> :information_source: chroot，即 change root directory (更改 root 目录)。在 linux 系统中，系统默认的目录结构都是以 `/`，即是以根 (root) 开始的。而在使用 chroot 之后，系统的目录结构将以指定的位置作为 `/` 位置。有关chroot的更多内容，可参阅：[理解 chroot](https://www.ibm.com/developerworks/cn/linux/l-cn-chroot/index.html)一文。

#### 配置用户登录权限

ftpusers是黑名单，文件中指定的用户不允许登录FTP服务器，通常是为了防止一些权限很高的用户做一些破坏性的事情，比如：root。查看ftpusers文件中的内容，可以看到默认会将一些特殊的账户写入到该文件中：

```shell
# Users that are not allowed to login via ftp
root
bin
daemon
adm
lp
sync
shutdown
halt
mail
news
uucp
operator
games
nobody
```

在vsftpd.conf中设置user_list黑白名单：

```shell
# 是否启用user_list文件配置
userlist_enable=YES
# 上面设置为YES时，该配置为NO表示user_list为白名单，YES则为黑名单
userlist_deny=NO
```

在vsftpd.conf中设置是否允许匿名/本地用户登录及是否运行写入：

```shell
# Allow anonymous FTP? (Beware - allowed by default if you comment this out).
anonymous_enable=NO
#
# Uncomment this to allow local users to log in.
# When SELinux is enforcing check for SE bool ftp_home_dir
local_enable=YES
#
# Uncomment this to enable any form of FTP write command.
write_enable=YES
#
# Default umask for local users is 077. You may wish to change this to 022,
# if your users expect that (022 is used by most other ftpd's)
local_umask=022
```

> :information_source: 匿名登录的用户名为ftp，密码为空，登陆后的位置是/var/ftp

#### chroot

在vsftpd.conf中配置chroot选项。以下配置，实现了除**chroot_list_file**中指定的用户之外其余所有用户均执行chroot操作。即，登录FTP服务器后，将用户家目录设置为根目录。这样，可以实现登录FTP后无法切换到上层目录的效果，因为家目录自己是最顶层的根目录。

```shell
# You may specify an explicit list of local users to chroot() to their home  
# directory. If chroot_local_user is YES, then this list becomes a list of   
# users to NOT chroot().
# (Warning! chroot'ing can be very dangerous. If using chroot, make sure that
# the user does not have write access to the top level directory within the  
# chroot)
chroot_local_user=YES
chroot_list_enable=YES
# (default follows)
chroot_list_file=/etc/vsftpd/chroot_list
allow_writeable_chroot=YES
```

**chroot_list_enable**表示是否启用chroot_list_file配置，chroot_list中的用户不受chroot_local_user设置的影响，属于例外情况，即：

+ chroot_local_user=YES

该设置表示所有用户登录FTP服务器时都执行chroot操作，但chroot_list中指定的用户除外

+ chroot_local_user=NO

该设置表示所有用户登录FTP服务器时均不执行chroot操作，但chroot_list中指定的用户除外

这里，我们将chroot_local_user设为YES，用户wjchi放到chroot_list文件中：

+ 使用wjchi登录FTP服务器并查看当前目录，会显示家目录

```shell
-bash-4.2$ ftp localhost
Trying ::1...
Connected to localhost (::1).
220 (vsFTPd 3.0.2)
Name (localhost:wjchi): wjchi
331 Please specify the password.
Password:
230 Login successful.
Remote system type is UNIX.
Using binary mode to transfer files.
ftp> pwd
257 "/home/wjchi"
ftp>
```

+ 使用xfh登录FTP并查看当前目录，会显示根目录

```shell
-bash-4.2$ ftp localhost
Trying ::1...
Connected to localhost (::1).
220 (vsFTPd 3.0.2)      
Name (localhost:wjchi): xfh
331 Please specify the password.
Password:
230 Login successful.
Remote system type is UNIX.
Using binary mode to transfer files.
ftp> pwd
257 "/"
ftp> 
```

> :warning: 这里虽然显示的是根目录，但操作的实际上是用户xfh的家目录：/home/xfh。上传/下载文件都是基于/home/xfh目录的。这里的根目录可阻止用户继续跳转到上级目录。



#### SCP vs SFTP

SCP(Secure copy)与FTP/SFTP都可以实现在不同计算机之间进行文件传输。二者之间的区别，可参考：

[What's the difference between SCP and SFTP?](https://superuser.com/questions/134901/whats-the-difference-between-scp-and-sftp)

**Compared to the earlier SCP protocol, which allows only file transfers, the SFTP protocol allows for a range of operations on remote files – it is more like a remote file system protocol**.

SCP仅仅用于文件的传输，SFTP除文件传输外还可以与远程访问进行一定的交互：

```shell
sftp> pwd
Remote working directory: /home/wjchi
sftp> ls -al
drwxr-xr-x    5 wjchi    root         4096 Jan 23 08:59 .
drwxr-xr-x    8 root     root         4096 Jan 19 13:32 ..
-rw-------    1 wjchi    wjchi        1967 Jan 23 11:11 .bash_history
drwxrwxr-x    3 wjchi    wjchi        4096 Jan 19 12:22 .cache
drwxrwxr-x    3 wjchi    wjchi        4096 Jan 19 12:22 .config
drwx------    2 wjchi    wjchi        4096 Jan 23 12:37 .ssh
-rw-------    1 wjchi    wjchi        3230 Jan 23 08:59 .viminfo
-rw-rw-r--    1 wjchi    wjchi           0 Jan 22 12:58 file
sftp>
```



#### SFTP vs FTP

以下内容摘录自知乎：[sftp与ftp是否没有区别？](https://www.zhihu.com/question/20402010)

> ftp是一个文件传输服务，设计它的目的就是为了传输文件。它有独立的守护进程，使用20，21两个端口，20是数据链路的端口，21是控制链路的端。
>
> sftp也是用来传输文件的，但它的传输是加密的，是ssh服务的一部分，没有单独的守护进程，是ssh服务的一部分，可以看做是ssh服务文件传输方案。和ssh一样，使用22端口。

**FTP is often [secured](https://en.wikipedia.org/wiki/File_Transfer_Protocol#Security) with [SSL/TLS](https://en.wikipedia.org/wiki/Transport_Layer_Security) ([FTPS](https://en.wikipedia.org/wiki/FTPS)) or replaced with [SSH File Transfer Protocol](https://en.wikipedia.org/wiki/SSH_File_Transfer_Protocol) (SFTP)**.

此外，FTP/FTPS有**主动模式和被动模式**，SFTP则不区分主动模式、被动模式。

## SSH

[SSH(Secure Shell)](https://zh.wikipedia.org/wiki/Secure_Shell)是一种加密的网络传输协议，和FTP一样，SSH也是C/S架构。通常，Linux发行版中内置了SSH的实现，如[OpenSSH](http://www.openssh.com/)。

#### 目录结构

客户端公钥通常放在登录**用户的家目录**的**authorized_keys**文件中，完整路径：**~/.ssh/authorized_keys**

```shell
# 将客户端生成的user用户的公钥发送到服务器上
ssh-copy-id -i ~/.ssh/pub_key user@host
```

centos中.ssh目录中主要包含以下几个文件：

```shell
[root@VM_0_2_centos ~]# ls -al ~/.ssh
total 16
drwx------   2 root root 4096 Jan 20 13:14 .
dr-xr-x---. 10 root root 4096 Jan 22 13:37 ..
-rw-------   1 root root 1155 Dec 18 12:27 authorized_keys
-rw-r--r--   1 root root  175 Dec 21 13:13 known_hosts    
[root@VM_0_2_centos ~]#
```

**authorized_keys**中用户存放客户端的公钥，known_hosts中存放已认证主机地址的指纹信息。一台计算机既可以是SSH客户端，也可以是SSH服务器，所以可以同时存在authorized_keys和known_hosts两个文件。

---

Windows中关于SSH的配置默认放在**c:\\users\\userId\\.ssh**文件夹中，主要包含以下几个文件：

```powershell
C:\Users\WenJun\.ssh>dir
 驱动器 C 中的卷是 OS
 卷的序列号是 60EC-5E31

 C:\Users\WenJun\.ssh 的目录

2020/01/20  21:21    <DIR>          .
2020/01/20  21:21    <DIR>          ..
2020/01/23  14:04               180 config
2019/12/09  23:09             1,679 id_rsa
2019/12/09  23:09               404 id_rsa.pub
2020/01/03  20:54             1,675 id_rsa_wjchi
2020/01/03  20:54               404 id_rsa_wjchi.pub
2019/12/17  23:05               178 known_hosts
```

**id_rsa/id_rsa.pub**和**id_rsa_wjchi/id_rsa_wjchi.pub**是两对私钥/公钥文件。config文件中的内容如下：

```shell
# Read more about SSH config files: https://linux.die.net/man/5/ssh_config
Host centos
   HostName remote_server_ip
   User wjchi
   # 用户的私钥文件地址，默认使用当前目录中的id_rsa
   IdentityFile C:\Users\WenJun\.ssh\id_rsa_wjchi
```

#### 登录

使用SSH登录服务器有两种方式：

- 用户名密码
- 公钥/私钥

有关SSH登录的更多内容，可参阅：[图解SSH原理](https://mp.weixin.qq.com/s?__biz=MzI3MTI2NzkxMA==&mid=2247487942&idx=1&sn=c46f510ff119bd0bdfe30689f0398854&chksm=eac530efddb2b9f9a8b1bf08454084d68b200dc7d4d999fa65d0d1ed69e78c386a26331533ea&mpshare=1&scene=1&srcid=0717tNKunOWIHONN04dsyFgm&sharer_sharetime=1565775380935&sharer_shareid=266dc9451fd28ecaad4697cc057771d2&key=f8a84d04edd3d2627c2d51c9c20364a8bd050f0a5ca9f829c5af707ece4562aacd7b4fdbbec1b7c84456c246b76557464b98cb1e0f54505984994db9af068c8e5f2bdd283b7ac92eac272e3a4432b665&ascene=1&uin=MTI5MDA0NDAwOA%3D%3D&devicetype=Windows+10&version=62060833&lang=zh_CN&pass_ticket=VfTZzibTuvTk6Edv7m13QePmjxNCsWe1iJ8Cn5e7klB%2B5DWIlJVve3rADmcSPJu6)一文。根据config中的配置我们可以直接使用`ssh Host`的方式通过公钥/私钥认证的方式登录远程服务器：

```shell
ssh centos
Last login: Thu Jan 23 08:08:09 2020 from 123.45.67.890
-bash-4.2$ whoami
wjchi      
-bash-4.2$ 
```



> :information_source: 通常在安装[git for windows](https://git-scm.com/download/win)客户端时，会一起安装ssh、ssh-keygen、sftp、scp等客户端工具

#### 配置

SSH的配置文件位于/etc/ssh目录中

```shell
-bash-4.2$ ls -al /etc/ssh
total 628
drwxr-xr-x.  2 root root   4096 Jan 19 12:51 .
drwxr-xr-x. 91 root root  12288 Jan 20 02:45 ..
-rw-r--r--   1 root root 581843 Aug  9 01:40 moduli
-rw-r--r--   1 root root   2276 Aug  9 01:40 ssh_config
-rw-------   1 root root   3937 Jan 13 02:12 sshd_config
-rw-------   1 root root    668 Dec 17 14:11 ssh_host_dsa_key        
-rw-r--r--   1 root root    608 Dec 17 14:11 ssh_host_dsa_key.pub    
-rw-------   1 root root    227 Dec 17 14:11 ssh_host_ecdsa_key      
-rw-r--r--   1 root root    180 Dec 17 14:11 ssh_host_ecdsa_key.pub  
-rw-------   1 root root    411 Dec 17 14:11 ssh_host_ed25519_key    
-rw-r--r--   1 root root    100 Dec 17 14:11 ssh_host_ed25519_key.pub
-rw-------   1 root root   1679 Dec 17 14:11 ssh_host_rsa_key        
-rw-r--r--   1 root root    400 Dec 17 14:11 ssh_host_rsa_key.pub    
-bash-4.2$
```

其中，ssh_config是对客户端的配置，sshd_config是对服务器端的配置（一台计算机既可以作为客户端，又可以做为服务器）。可以在sshd_config中限制root用户直接通过SSH连接到服务器：

```shell
#LoginGraceTime 2m
PermitRootLogin no
#StrictModes yes
#MaxAuthTries 6
#MaxSessions 10

#PubkeyAuthentication yes

# The default is to check both .ssh/authorized_keys and .ssh/authorized_keys2
# but this is overridden so installations will only check .ssh/authorized_keys
AuthorizedKeysFile .ssh/authorized_keys
```



## 推荐阅读

[理解 chroot](https://www.ibm.com/developerworks/cn/linux/l-cn-chroot/index.html)

[图解SSH原理](https://mp.weixin.qq.com/s?__biz=MzI3MTI2NzkxMA==&mid=2247487942&idx=1&sn=c46f510ff119bd0bdfe30689f0398854&chksm=eac530efddb2b9f9a8b1bf08454084d68b200dc7d4d999fa65d0d1ed69e78c386a26331533ea&mpshare=1&scene=1&srcid=0717tNKunOWIHONN04dsyFgm&sharer_sharetime=1565775380935&sharer_shareid=266dc9451fd28ecaad4697cc057771d2&key=f8a84d04edd3d2627c2d51c9c20364a8bd050f0a5ca9f829c5af707ece4562aacd7b4fdbbec1b7c84456c246b76557464b98cb1e0f54505984994db9af068c8e5f2bdd283b7ac92eac272e3a4432b665&ascene=1&uin=MTI5MDA0NDAwOA%3D%3D&devicetype=Windows+10&version=62060833&lang=zh_CN&pass_ticket=VfTZzibTuvTk6Edv7m13QePmjxNCsWe1iJ8Cn5e7klB%2B5DWIlJVve3rADmcSPJu6)

[Linux使用ssh超时断开连接的真正原因](http://bluebiu.com/blog/linux-ssh-session-alive.html)

[sftp与ftp是否没有区别？](https://www.zhihu.com/question/20402010)

[Differences between SFTP and “FTP over SSH”](https://stackoverflow.com/questions/440463/differences-between-sftp-and-ftp-over-ssh)