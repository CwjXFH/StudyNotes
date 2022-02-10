使用TypeScript或者ES2015+标准中的`extends`关键字是很容易实现继承的，但这不是本文的重点。JS使用了基于原型（**prototype-based**）的继承方式，`extends`只是语法糖，本文重点在于不使用`extends`来自己实现继承，以进一步理解JS中的继承，实际工作中肯定还是要优先考虑使用`extends`关键字的。

## 原型 & 原型链

原型用于**对象属性**的查找。画出下面代码中的原型链图示：

```js
class Person {
    private _name: string;

    constructor(name: string) {
        this._name = name;
    }

    get getName(): string {
        return this._name;
    }
}

let person = new Person("xfh");
```

![](imgs/prototype_chain.png)

> 图中,\_\_proto\_\_表示实例的原型对象，prototype表示构造函数的原型对象。不再推荐使用\_\_proto\_\_，将来可能会被废弃，可使用[Object.getPrototypeOf()]([Object.getPrototypeOf() - JavaScript | MDN (mozilla.org)](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/getPrototypeOf))来获取对象的原型。

关于原型/链，记住以下几点：

+ 原型链的终点是null，从这个角度，可以将null看作所有Object的基类
+ 实例的原型对象和它构造函数的原型对象是同一个对象（比较拗口）
+ 所有的函数（包括构造函数及Function自身）都是Function的实例
+ 函数是普通的对象，只是具备了可调用（callable）功能 ，想到了Python中的类装饰器，也是具备了可调用功能的普通类
+ 所有的对象终归是Object的实例，即Object位于所有对象的原型链上

````js
// 原型链的终点是null
Object.getPrototypeOf(Object.prototype)===null // true
Object.prototype instanceof Object // false
// 实例和构造函数的原型对象是同一个对象
Object.getPrototypeOf(Function)===Function.prototype // true
// 所有的函数（包括构造函数及Function自身）都是Function的实例
Function instanceof Function // true，Function是自己的实例
Object instanceof Function // true，构造函数是Function的实例
// 所有的对象终归是Object的实例，即Object位于所有对象的原型链上
Function.prototype instanceof Object // true
Function instanceof Object // true
Object instanceof Object // true
````

typeof`操作符与`instanceof`关键字的区别如下：

> Keep in mind the only valuable purpose of `typeof` operator usage is checking the Data Type. If we wish to check any Structural Type derived from Object it is pointless to use `typeof` for that, as we will always receive `"object"`. The indeed proper way to check what sort of Object we are using is [`instanceof`](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/instanceof) keyword. But even in that case there might be misconceptions.

## 实现继承

JS中对象成员分为三类：实例、静态、原型。实例成员绑定到具体实例上（通常是this上），静态成员绑定到构造函数上，原型成员就存在原型对象上：

```js
/**
 * 从基类继承成员
 * @param child 子类构造函数或实例
 * @param base 基类构造函数或实例
 */
function inheritMembers(child, base) {
    let ignorePropertyNames = ["name", "caller", "prototype", "__proto__", "length", "arguments"];
    let propertyNames = Object.getOwnPropertyNames(base);
    for (let propertyName of propertyNames) {
        if (ignorePropertyNames.includes(propertyName)) {
            continue;
        }
        let descriptor = Object.getOwnPropertyDescriptor(base, propertyName);
        if (!descriptor) {
            continue;
        }
        Object.defineProperty(child, propertyName, descriptor);
    }
}
/**
 * 从基类继承原型及静态成员
 * @param thisCtor 子类构造函数
 * @param baseCtor 基类构造函数
 */
function inheritSharedMembers(thisCtor, baseCtor) {
    if (typeof thisCtor !== "function" || typeof baseCtor !== "function") {
        throw TypeError("参数必须是函数：thisCtor,baseCtor");
    }
    // 继承原型成员
    thisCtor.prototype = Object.create(baseCtor.prototype);
    thisCtor.prototype.constructor = thisCtor;
    // 继承静态成员
    inheritMembers(thisCtor, baseCtor);
}
/**
 * 调用子类及父类构造函数创建子类实例，并继承父类实例成员（这也是调用父类构造函数的原因）
 * @param thisInstance 子类实例
 * @param baseInstance 父类实例
 */
function createInstance(thisInstance, baseInstance) {
    inheritMembers(thisInstance, baseInstance);
    return thisInstance;
}

// 构造函数
function Animal(tag) {
    // 实例属性
    this.tag = tag;
}
// 静态方法，需通过构造函数来调用
Animal.bark = function () {
    console.log("static func, this= " + this + ", typeof this=" + typeof this);
};
// 原型方法，需通过实例来调用
Animal.prototype.getInfo = function () {
    console.log("property func, tag:" + this.tag);
};

function Dog(name = null) {
    this.name = name ?? "default";
}
// 添加子类原型方法
Dog.prototype.dogBark = function () {
    console.log("dog bark");
};
// 继承父类原型及静态成员
inheritSharedMembers(Dog, Animal);

var animal = new Animal("animal");
Animal.bark();
// TypeError: animal.bark is not a function
// animal.bark();
animal.getInfo();
// property getInfo not exist on type 'typeof Animal'
// Animal.getInfo();


let dog = createInstance(new Dog("dog"), new Animal("dog"));

dog.getInfo();
dog.dogBark();
Dog.bark();
console.log(dog.name);
```



最后使用`v4.1.3`版本的TS，编译为ES5版本的JS，看看TS背后是如何实现继承的：

```typescript
class Person {
    name: string;
    age: number;
    constructor(name: string, age: number) {
        // 只能在构造函数中使用this关键字
        this.name = name;
        this.age = age;
    }
    // 静态方法中调用本类中的另一个静态方法时，可以使用this.methodName的形式
    // 在外部调用时只能类名.方法名的形式，所以此时方法内部，this是指向构造函数的
    // 即，this.methodName等价于类名.方法名
    static static_method() {
        // 这里this指向Person类，typeof this=function
        // 可以看出class Person本质上是构造函数，class只是语法糖
        console.log(`static method, this=${this}, typeof this=${typeof this}`);
    }
}

// 使用extends继承
class Chinese extends Person {
    constructor(name: string, age: number) {
        // 必须调用父类构造函数，且需要在子类构造函数使用this关键字之前调用，否则会产生错误：
        // A 'super' call must be the first statement in the constructor when a class contains initialized properties or has parameter properties.
        super(name, age);
    }

    sayHello() {
        console.log(`I'm ${this.name}, I'm ${this.age} years old.`)
    }
}


let cn = new Chinese('xfh', 26);

cn.sayHello();
Chinese.static_method();
```

编译后代码如下：

```js
"use strict";
var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var Person = /** @class */ (function () {
    function Person(name, age) {
        // 只能在构造函数中使用this关键字
        this.name = name;
        this.age = age;
    }
    // 静态方法中调用本类中的另一个静态方法时，可以使用this.methodName的形式
    // 在外部调用时只能类名.方法名的形式，所以此时方法内部，this是指向构造函数的
    // 即，this.methodName等价于类名.方法名
    Person.static_method = function () {
        // 这里this指向Person类，typeof this=function
        // 可以看出class Person本质上是构造函数，class只是语法糖
        console.log("static method, this=" + this + ", typeof this=" + typeof this);
    };
    return Person;
}());
// 使用extends继承
var Chinese = /** @class */ (function (_super) {
    __extends(Chinese, _super);
    function Chinese(name, age) {
        // 必须调用父类构造函数，且需要在子类构造函数使用this关键字之前调用，否则会产生错误：
        // A 'super' call must be the first statement in the constructor when a class contains initialized properties or has parameter properties.
        return _super.call(this, name, age) || this;
    }
    Chinese.prototype.sayHello = function () {
        console.log("I'm " + this.name + ", I'm " + this.age + " years old.");
    };
    return Chinese;
}(Person));
var cn = new Chinese('xfh', 26);
cn.sayHello();
Chinese.static_method();
```



## 推荐阅读

[JavaScript data types and data structures](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Data_structures)  
[Object.prototype.\_\_proto\_\_](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/proto)  
[Object prototypes](https://developer.mozilla.org/en-US/docs/Learn/JavaScript/Objects/Object_prototypes)