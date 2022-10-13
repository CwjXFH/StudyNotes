class Animal:
    def __init__(self):
        self.__tag = "animal"


class Person(Animal):
    def __init__(self):
        self.__private()

    def __private(self):
        print('call private method')


p = Person()

print(p)
print(Person)
print(Animal)
