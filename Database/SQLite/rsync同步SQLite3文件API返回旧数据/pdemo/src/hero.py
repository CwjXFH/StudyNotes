from entity import Entity


class Hero(Entity):
    def __init__(self, id, name):
        Entity.__init__(self, 'Heros')
        self.__id = id
        self.name = name

    @property
    def id(self):
        return self.__id
