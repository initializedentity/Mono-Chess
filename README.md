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

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/901e40fa-2daa-4109-a429-c78e383a9b89)


Imagem 3 - Função LoadContent()

Nesta função, tem o código para carregar o jogo para a memória, onde se cria um ecrã falso, e depois colamos os nossos assets em cima desse ecrã falso, numa largura fixa de 1920 píxeis, e uma largura dependente do aspect ratio atual. Também temos o carregamento da fonte que se utilizou para o texto de GAME OVER.

> [!NOTE]
> Este jogo suporta aspect ratios de 4:3, 16:9 e 16:10.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/8f80adca-a102-41d9-bdb0-e6f9f5b2eded)

Imagem 4 - Função Update()

Nesta função, temos código para input de algumas teclas úteis.

F11 tira e coloca o jogo em ecrã inteiro;

Esc fecha o jogo;

R dá reset ao tabuleiro.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/187e5e65-ac39-4ee0-8e6a-949f1ebe76f2)

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/d82ec907-b13e-4f29-96d3-e31fe0a6af47)

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/14f80a23-735d-4d79-95ac-04fe7492c991)

Imagem 5, 6 e 7 - Função Draw()

Nesta função, temos apenas código para renderizar o que foi carregado em memória na função LoadContent(). Também existe código que faz com que quando selecionamos uma peça, os quadrados para os quais sejam possíveis mexer ficam iluminados em azul, e quando o rei se encontra em cheque. Também existe aqui a implementação do fim de jogo e o texto de GAME OVER, onde diz quem é ganhou e para carregar no R para reiniciar o jogo.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/293ef6ed-c4fc-40e6-8662-e85dc54042d6)

Imagem 8 - Função ScalePositionRenderTarget()

Nesta função, temos código para calcular a resolução do monitor, e utiliza-se a menor para renderizar o conteúdo visual para a janela do jogo. Temos também código para fazer com que isso fique centrado.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/9c25cbfb-25f8-4c1b-8e97-79f16a5cb0b2)

Imagem 9 - Função WindowSizeChanged()

Nesta função, temos código para verificar se a aspect ratio do jogo foi alterada, e voltar a desenhar o ecrã falso, caso tenha sido alterada.
Também temos uma prevenção de cálculo constante caso o utilizador esteja a arrastar a janela.

Pieces.cs

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/a18eda02-3d0b-4165-b83c-4c8c352e9ec4)

Imagem 10 - Construtor Pieces

Neste construtor, temos apenas inicializações de variáveis que se vão utilizar mais tarde, que incluem o delay de input do rato e um vetor que diz ao programa em que peça estamos a clicar.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/6dfd77bb-baaf-4401-bd09-75a0e7d9f9d5)

Imagem 11 - Função LoadContent()

Esta função, ao contrário da LoadContent() anterior, contém código para dar load às sprites das peças e dos efeitos sonoros utilizados.

> [!NOTE]
> O tabuleiro é carregado no Game1.cs, desenhado totalmente pelo computador, utilizando um scaling de 1/64 para cada quadrado.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/5355fc16-55e9-4db9-a368-8973d01023ce)

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/dad2ca68-f032-4955-adc7-4a8370b0df0c)

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/ff8edac9-cebf-453a-bab0-ecf806529af6)

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/36998f36-e435-4382-afc8-b9845e9e4404)

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/b316024b-8d58-467f-bbd8-cc4fcc33c73a)

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/0bf0af9c-0c3c-4231-9b3c-3f911c5d02d0)

Imagens 12, 13, 14, 15, 16 e 17 - Função Update()

Nesta função, temos código para gerir input do rato, calcular onde o cursor está na resolução real (isto porque o rato tem de interagir com a resolução real do jogo), código para quando pegamos numa peça, consigamos trocá-la de sítio e que faça um som quando o fazemos, código para evitar que o jogador adversário consiga mexer nas nossas peças e as regras de promoção para os peões, onde é preciso clicar nas teclas 1, 2, 3 ou 4 para promover o peão para a peça desejada.

> [!NOTE]
> 1 é para a torre, 2 é para o cavalo, 3 é para o bispo e 4 é para a rainha.

> [!WARNING]
> A nota acima indica uma funcionalidade que não está funcional no momento em que este relatório foi escrito, e o rei consegue dar cheque a si próprio, onde a solução para este problema em particular é "comer" o rei em si.

Também verifica se o rei está em cheque e se aconteceu um chequemate.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/abd2ae6f-ed09-4a9a-9fae-38080849ab55)

Imagem 18 - Função Draw()

Nesta função, temos mais do mesmo que a outra função Draw(), ou seja, renderizamos os conteúdos já carregados para o ecrã.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/60b6c2c5-6d9d-4dd0-a3b6-fb1dfd99cc0a)

Imagem 19 - Função SpritePiece()

Nesta função, temos código para colocar as peças no tabuleiro.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/ed3e498b-6b6b-47b4-9391-98acaf7cbc41)

Imagem 20 - Função UpdateResolution()

Nesta função, temos código para lidar com as várias resoluções que o jogo suporta.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/1b319232-eaac-438e-aace-b6487256eb97)

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/62c9f025-f0eb-4f4e-bf98-deaeb714b946)

Imagem 21 e 22 - Função ResetBoard()

Nesta função, temos código para voltar a colocar cada peça na sua posição padrão.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/9b453486-ed3f-4afa-bdd1-1881f180d4bf)

Imagem 23 - Função ResetLegalMoves()

Nesta função, temos código que faz com que se dê reset à matriz que determina as posições legais da peça atual.

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/cc384ef0-13ae-4fac-aafe-72cf6d953fcc)

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/48a40419-2a3c-437b-91f0-7446e14aebc9)

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/22478163-fe59-456e-99ab-795031ce5816)

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/74a7f78f-59cc-44fb-a4ab-4ed8363fb508)

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/b51f23ba-da52-4a4c-9511-735a6e80a042)

![image](https://github.com/initializedentity/Mono-Chess/assets/106490681/ddabcb26-18bb-42e2-8943-70b168102654)

Imagens 24, 25, 26, 27, 28 e 29 - Função LegalMoves()

Nesta função, existe todo o código responsável pelas regras impostas a cada peça.

**Program.cs**

Aqui temos apenas o código que vai ativar quando abrimos o jogo.
