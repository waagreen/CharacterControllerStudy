Nesse projeto busquei demonstrar os principios SOLID de responsabilidade única (S) e segregação de interface (I) através da implementação de uma "máquina hierarquica de estados finitos" para o controle de personagem em ambiente 3D. Utilizei a Unity e a linguagem C#. 

A ideia é que o personagem só possa estar em um sub-estado e super-estado ao mesmo tempo, por exemplo, logo após realizar um pulo o super-estado seria "no ar" e o sub-estado "andando". Dessa forma é possível isolar comportamentos específicos com responsabilidades únicas em seus devidos scripts como pulo, movimento, ataque, parado e etc. Cada um desses scripts só depende da máquina de estados e diz se é possível ou não transicionar para outro estado e fica responsável por dizer quando isso acontece.

Toda está estrutura está contida pasta "Assets/Scripts/States", onde é possível observar o benefício da segregação de interface, pois todos os estados são extenções da classe abstrata "State" então podem ser tratados de maneira intercambiável entre si no código. 

____________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________


In this project, I sought to demonstrate the SOLID principles of Single Responsibility (S) and Interface Segregation (I) through the implementation of a "hierarchical finite state machine" for controlling a character in a 3D environment. I used Unity and the C# language.

The idea is that the character can only be in one sub-state and one super-state at the same time. For example, right after performing a jump, the super-state would be "in the air", and the sub-state would be "walking". This way, it’s possible to isolate specific behaviors with single responsibilities in their respective scripts, such as jumping, movement, attacking, idling, etc. Each of these scripts only depends on the state machine and determines whether it’s possible to transition to another state, while also being responsible for defining when that transition happens.

This entire structure is contained in the "Assets/Scripts/States" folder, where the benefit of interface segregation is evident, as all states are extensions of the abstract class "State", allowing them to be treated interchangeably in the code.
