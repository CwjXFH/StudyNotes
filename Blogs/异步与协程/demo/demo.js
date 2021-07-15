// JS中async/await基于Promise+协程的方式来实现异步编程，await不会阻塞主线程
// 调用async函数会返回Promise对象
// 遇到await表达式时，会暂停当前协程的执行，等待await表达式运算完成
// await表达式：
//      1. 如果await后面是Promise对象，则当Promise对象的状态为fulfill/reject时，
//      await表达式结束等待，await后面的代码将被执行
//
//      2. 如果await后面不是Promise对象，则隐式包装为状态为fulfill的Promise对象

async function PromiseAll(values) {
    // console.log('call promise all');
    let result = [];
    for (let i = 0; i < values.length; i++) {
        await Promise.resolve(values[i]).then(value => {
            (async function () {
                let index = i;
                let tmpResult = await value;
                result[index] = tmpResult;
            })();
        });
    }

    // console.log('waiting result');
    if (result.length == values.length) {
        // console.log('promise all result', result);
        return result;
    }
}


async function func1() {
    // console.log('call func1');
    await new Promise(resolve => {
        setTimeout(resolve, 3000);
    });
    return 100;
}

async function func2() {
    // console.log('call func2');
    await new Promise(resolve => {
        setTimeout(resolve, 3000);
    });
    return 200;
}

(async function () {
    console.log('before await');
    let result = [];
    result = await PromiseAll([func1(), func2()]);
    // result = await Promise.all([func1(), func2()]);

    // result[0] = await func1();
    // result[1] = await func2();
    // console.log(`func1() instanceof Promise: ${func1() instanceof Promise}`);
    // func2();
    console.log('after await');
    console.log('result', result);
})();
console.log('end...');
