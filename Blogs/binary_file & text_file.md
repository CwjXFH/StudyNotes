## 文件分类

文件分为两类：二进制文件和文本文件。所有数据在计算机中均以二进制形式存在，这里所说的二进制和文本是以程序解释文件数据的方式来区分的。

### 二进制文件

二进制文件中通常被看作是一个字节序列，文件中的数据会被应用程序解释为文本字符之外的东西，常见的二进制文件有：pdf、exe、图片等等。

通常，二进制文件会包含一些[头信息](https://en.wikipedia.org/wiki/Header_(computing))或者[元数据](https://en.wikipedia.org/wiki/Metadata)以便于应用程序来解释文件中的数据，具体将数据解释为什么内容还需结合应用程序自身的逻辑，一个字节既可以是字符，也可是音视频。头信息通常包含[signature or *magic* number](https://en.wikipedia.org/wiki/List_of_file_signatures)用来确定文件数据格式。不含有头信息的二进制文件，一般叫做**float binary file**。

在一些场景下，如邮件，无法直接发送二进制数据，这时需要将二进制数据通过[Binary-to-text encoding](https://en.wikipedia.org/wiki/Binary-to-text_encoding)协议转换为文本。

### 文本文件

相较于二进制文件，文本文件比较简单。通常不会像二进制文件那样含有一些头信息，但在读写文件时需要指明指明所用字符集与编码方式。

## 操作文件

编程语言对于二进制文件和文本文件的处理会有不同的方式，如，使用参数来区分或者使用不同的方法重载。

#### JavaScript

> 参考[FileReader](https://developer.mozilla.org/en-US/docs/Web/API/FileReader)

```javascript
<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Document</title>
</head>

<body>
    <input type="file" id="file" value="选择文件" />
    <input type="button" value="上传" onclick="getFile()" />
    <script>
        function getFile() {
            let selectedFile = document.getElementById('file').files[0];

            var reader = new FileReader();
            reader.onload = function () {
                console.log(reader.result);
                let buffer = new Uint8Array(reader.result);
				console.log(buffer);
            };
            // 将文件内容转为字节序列
            reader.readAsArrayBuffer(selectedFile);
            // 以文本形式读取文件
        	// reader.readAsText(selectedFile,'utf-8');
        }

    </script>
</body>

</html>
```

#### Python

> 参考[io模块](https://docs.python.org/3.4/library/io.html?highlight=io)
>
> Github上一个获取二进制文件信息的Python项目：[Binary File Info](https://github.com/praise002/20-python-project/tree/master/binary-file-info)

```python
import os
import io

current_dir = os.getcwd()

token_file_path = f'{current_dir}/token'

# 读取文本文件需指明编码方式
with open(token_file_path, 'r', encoding='utf8') as file:
    token = file.read()

# 使用二进制方式读写文件
with open(token_file_path, 'rb') as file:
    b_token = file.read()

with open(token_file_path, 'wb') as file:
    file.write(b_token)
    
img_file_path = f'{current_dir}/demo/imgs/js.PNG'
with open(img_file_path, 'rb') as file:
    img_context = file.read()
    img_bytes = bytearray(img_context)
    # list(img_context)
    # print(img_context[0])
    # list(img_bytes)
    # print(img_bytes[0])

with open(f'{current_dir}/demo/src/img_file.png', 'wb') as file:
    file.write(img_context)
```

#### C#

```c#
using var fileStream = File.Open(@"C:\Users\WenJun\Desktop\img.PNG", FileMode.Open, FileAccess.Read, FileShare.Read);
// File.OpenText("");
using var streamReader = new StreamReader(fileStream, true);
//using var streamReader = new StreamReader(fileStream, Encoding.UTF8);
//while (streamReader.ReadLine()>0)
//{

//}
var img_str = streamReader.ReadToEnd();
```



## 推荐阅读

[Binary file](https://en.wikipedia.org/wiki/Binary_file)

[List of file signatures](https://en.wikipedia.org/wiki/List_of_file_signatures)

[Text file](https://en.wikipedia.org/wiki/Text_file)