# Mono-Chess
Um jogo de xadrez simples, feito em C# e com a framework Monogame.

Feito por: 

Dário Ribeiro, a23489

Ricardo Lopes, a22337

**Introdução**

Com o objetivo de desenvolver um jogo em Monogame, decidiu-se desenvolver um jogo de xadrez, onde se pode jogar o clássico jogo de tabuleiro.

> [!NOTE]
> Este jogo precisa de 2 jogadores para ser jogado, pois não contém um CPU.

> [!WARNING]  
> Para jogar este jogo, é necessário ter o Monogame instalado.

**Discussão**

Para desenvolver este jogo, decidiu-se não complicar demasiado as coisas, e fazer o código de maneira simples e eficaz.

O jogo contém:

-Código responsável por cada peça;

-Código usado apenas para debugging;

-Código necessário para renderizar o conteúdo visual do jogo para o ecrã;

-Código para utilizar efeitos sonoros.

**Análise de resultados**

Game1.cs


![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/7a60345b-18c6-411e-a079-0a1e1f642ead)

Imagem 1 - Função Game1()

Nesta função, apenas temos código para editar a janela onde o jogo se encontra, se o cursor do rato fica visível durante a playthrough e um delay imposto para prevenir spam de inputs.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/9c66f0d2-4778-48ab-9055-c919467027aa)

Imagem 2 - Função Initialize()

Nesta função, existe código apenas para iniciar o jogo, ou caso a janela não estiver em ecrã inteiro, coloca-a na resolução padrão (sendo esta 1920x1440, em aspect ratio 4:3).

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/634ad84f-a013-4eee-ad9f-bd422b4b7ba8)

Imagem 3 - Função LoadContent()

Nesta função, tem o código para carregar o jogo para a memória, onde se cria um ecrã falso, e depois colamos os nossos assets em cima desse ecrã falso, numa largura fixa de 1920 píxeis, e uma largura dependente do aspect ratio atual.

> [!NOTE]
> Este jogo suporta aspect ratios de 4:3, 16:9 e 16:10.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/bbd4d727-15cb-495a-9ad3-e7b7b718095a)

Imagem 4 - Função Update()

Nesta função, temos código para input de algumas teclas úteis.

F11 tira e coloca o jogo em ecrã inteiro;

Esc fecha o jogo;

R dá reset ao tabuleiro.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/8a3433f0-74e2-4c79-9375-aed56faed971)

Imagem 5 - Função Draw()

Nesta função, temos apenas código para renderizar o que foi carregado em memória na função LoadContent().

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/293ef6ed-c4fc-40e6-8662-e85dc54042d6)

Imagem 6 - Função ScalePositionRenderTarget()

Nesta função, temos código para calcular a resolução do monitor, e utiliza-se a menor para renderizar o conteúdo visual para a janela do jogo. Temos também código para fazer com que isso fique centrado.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/9c25cbfb-25f8-4c1b-8e97-79f16a5cb0b2)

Imagem 7 - Função WindowSizeChanged()

Nesta função, temos código para verificar se a aspect ratio do jogo foi alterada, e voltar a desenhar o ecrã falso, caso tenha sido alterada.
Também temos uma prevenção de cálculo constante caso o utilizador esteja a arrastar a janela.

Pieces.cs

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/a18eda02-3d0b-4165-b83c-4c8c352e9ec4)

Imagem 8 - Construtor Pieces

Neste construtor, temos apenas inicializações de variáveis que se vão utilizar mais tarde, que incluem o delay de input do rato e um vetor que diz ao programa em que peça estamos a clicar.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/6dfd77bb-baaf-4401-bd09-75a0e7d9f9d5)

Imagem 9 - Função LoadContent()

Esta função, ao contrário da LoadContent() anterior, contém código para dar load às sprites das peças e dos efeitos sonoros utilizados.

> [!NOTE]
> O tabuleiro é carregado no Game1.cs, desenhado totalmente pelo computador, utilizando um scaling de 1/64 para cada quadrado.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/8b393c3c-34c7-452c-b75b-f8de76f1c9a7)

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/4b9a7618-59de-4e48-b1b3-48e9c78535e6)

Imagens 10 e 11 - Função Update()

Nesta função, temos código para gerir input do rato, calcular onde o cursor está na resolução real (isto porque o rato tem de interagir com a resolução real do jogo), e código para quando pegamos numa peça, consigamos trocá-la de sítio e que faça um som quando o fazemos. Também temos código para evitar que o jogador adversário consiga mexer nas nossas peças.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/abd2ae6f-ed09-4a9a-9fae-38080849ab55)

Imagem 12 - Função Draw()

Nesta função, temos mais do mesmo que a outra função Draw(), ou seja, renderizamos os conteúdos já carregados para o ecrã.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/60b6c2c5-6d9d-4dd0-a3b6-fb1dfd99cc0a)

Imagem 13 - Função SpritePiece()

Nesta função, temos código para colocar as peças no tabuleiro.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/ed3e498b-6b6b-47b4-9391-98acaf7cbc41)

Imagem 14 - Função UpdateResolution()

Nesta função, temos código para lidar com as várias resoluções que o jogo suporta.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/1b319232-eaac-438e-aace-b6487256eb97)

Imagem 15 - Função ResetBoard()

Nesta função, temos código para voltar a colocar cada peça na sua posição padrão.

**Program.cs**

Aqui temos apenas o código que vai ativar quando abrimos o jogo.
