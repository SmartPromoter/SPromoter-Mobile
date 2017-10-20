# SmartPromoter - Mobile Xamarin Version
**SaaS OpenSource de monitoramento de equipes em campo no PDV.**

<a href="http://www.smartpromoter.trade/" title="Visite o website da SmartPromoter">
  <img src="http://www.smartpromoter.trade/wp-content/uploads/2014/11/SP-tablet-Mockup-1.png" alt="">
</a>

<hr/>
 
> **Warning:** O App precisa de um storage de imagens configurado e com as devidas autorizações de acesso para baixar e enviar os JPEGs.


**Features**
- APP desenvolvido em Xamarin com camada de negócios e banco de dados desenvolvida no PCL, e interfaces nativas respectivamente para Android e iOS
- Integração com câmera nativa e sistema de envio automático de fotos para blob storage
- Compactação de imagem instantânea para minimizar custo de armazenamento e envio.
- Formulário totalmente dinâmico para questionários de até 30 perguntas (variações de respostas - DateTime, Checkbox, Text string, Text Numerico, Slider e etc )
- Controle de geolocalização
- Sincronização automática de dados
- Integração com multiusuários para promotores compartilhados.


### Requisitos mínimos do usuário final

|         Modelo        |       Versão        | 
| --------------------- | ------------------- |
|       `Android`       |       API>=5.0      |
|         `iOS`         |  Iphone + iOS>10.0  |


### Configuração Básica

| Variável                 | Descrição                  | Classe                                                 |
| ------------------------ | -------------------------- | ------------------------------------------------------ |
| `url `                   | Endereco do Blob           | SPromoterMobile/Models/Credenciais/AzureCredenciais.cs |
| `key `                   | Chave de seguranca do Blob | SPromoterMobile/Models/Credenciais/AzureCredenciais.cs |
| `user `                  | ID do usuario do Blob      | SPromoterMobile/Models/Credenciais/AzureCredenciais.cs |
| `urlGetInstancia  `      | URL base do host da API    | SPromoterMobile/Models/Credenciais/API_Credenciais.cs  |
| `customMailUserDomain  ` | Nome do host do email      | SPromoterMobile/Models/Credenciais/API_Credenciais.cs  |


## Contribuição

Para mais informações, veja o [documento oficial de contribuição](https://github.com/SmartPromoter/SPromoter-Mobile/blob/master/CONTRIBUTING.md).


## Roadmap

Veja o nosso [roadmap](https://github.com/SmartPromoter/SPromoter-Mobile/blob/master/roadmap.md) para conferir o mesmo atualizado


## Copyright

Veja [LICENSE](https://github.com/SmartPromoter/SPromoter-Mobile/blob/master/LICENSE) para mais detalhes
