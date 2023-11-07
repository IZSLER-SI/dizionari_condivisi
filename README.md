# riusodizionaricondivisi

.NET Framework 4.7.2 <br/>
CMS: [DotNetNuke 9.9.1 aka DNN](https://github.com/dnnsoftware/Dnn.Platform/releases/tag/v9.9.1)

## Impostazione dei permessi IIS
1. Clonare il repository del riuso
2. Impostare IIS, in particolare *Application pool/Pool di applicazioni* e *Site/Siti*
3. Impostare i permessi alla cartella

## Creazione dei databases
Il database con desinenza _dnn conterrà il necessario per il mantenimento del CSM DNN
Il database senza desinenza conterrà le tabelle d'applicativo
```sql
CREATE DATABASE riusodizionari_dnn;
CREATE DATABASE riusodizionari;
```

### Creazione tabelle di base per riusodizionari_dnn
Il database sarà popolato durante l'installazione del CMS DNN. La stringa di connessione nel file web.config **deve** chiamarsi *SiteSqlServer*.

### Creazione tabelle di base per riusodizionari
La stringa di connessione nel file web.config **deve** chiamarsi *dizionari_condivisi*.
* Tabella elenco_dizionari: contiene l'elenco dei dizionari ed una serie di informazioni utili sul dizionario stesso
```sql
CREATE TABLE [dbo].[elenco_dizionari](
	[dizionario] [nvarchar](255) NOT NULL,
	[descrizione] [nvarchar](255) NULL,
	[chiave] [nvarchar](255) NULL,
	[data_ultima_modifica] [datetime] NULL,
	[num_record] [int] NULL,
	[utente_creazione] [nvarchar](100) NULL,
	[utente_ultima_modifica] [nvarchar](100) NULL,
	[extend] [text] NULL,
	[time_modifica] [datetime] NULL,
	[record_attivo] [tinyint] NULL,
	[campi_modificabili] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[dizionario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[chiave] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
```
* Tabella log_operazione: contiene log su alcune operazioni svolte
```sql
CREATE TABLE [dbo].[log_operazione](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[anno] [int] NULL,
	[utente] [nvarchar](255) NULL,
	[modulo] [nvarchar](255) NULL,
	[operazione] [nvarchar](255) NULL,
	[status] [nvarchar](5) NULL,
	[dizionario] [nvarchar](255) NULL,
	[dati] [text] NULL,
	[data_operazione] [datetime] NULL,
	[extend] [text] NULL,
	[time_modifica] [datetime] NULL,
	[record_attivo] [tinyint] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
```
* Tabella log_stato_arte_email: contiene informazioni sull'invio di email agli utenti "che osservano" un dizionario
```sql
CREATE TABLE [dbo].[log_stato_arte_email](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[utente] [nvarchar](100) NULL,
	[dizionario] [nvarchar](100) NULL,
	[id_log_ultimo_invio] [int] NULL,
	[extend] [text] NULL,
	[time_modifica] [datetime] NULL,
	[record_attivo] [tinyint] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
```
* Tabella ruolo_dizionario: contiene i ruoli di un singolo utente sul singolo dizionario
```sql
CREATE TABLE [dbo].[ruolo_dizionario](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[nome_utente] [nvarchar](100) NOT NULL,
	[dizionario] [nvarchar](255) NOT NULL,
	[super_user] [bit] NULL,
	[extend] [text] NULL,
	[time_modifica] [datetime] NULL,
	[record_attivo] [tinyint] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[ruolo_dizionario]  WITH CHECK ADD FOREIGN KEY([dizionario])
REFERENCES [dbo].[elenco_dizionari] ([dizionario])
GO
```

## Configurazione progetto
Accedere, quindi, al sito (URL è stato definito durante la configurazione di IIS) ed eseguire la procedura guidata di installazione del CSM DNN.

Installare il tema, nel caso in cui si voglia utilizzare un tema diverso da quello di default

Nella cartella ConfigProject ci sono delle funzionalità che andranno spostate, in funzione del tema in uso. Le funzionalità in questione sono
* [alertifyjs](https://alertifyjs.com/)
* [DataTables/DataTables-1.10.20](https://www.datatables.net/)
* jQueryUI-custom
* [moment-2.24.0](https://momentjs.com/)
* scripts/ivf.js: inizializzazione di funzionalità comuni, come datepicker o tabelle datatable

Acquistare [Editor Datatables v 1.9.2](https://editor.datatables.net/) e seguire la [guida d'installazione per .NET Framework](https://editor.datatables.net/manual/net/installing)

Per abilitare il login traminte Azure Active Directory, installare l'estensione [Azure AD Provider v 4.0.5](https://github.com/davidjrh/dnn.azureadprovider/releases/tag/v4.0.5) e seguire la [guida](https://github.com/davidjrh/dnn.azureadprovider)

## Configurazione moduli
### DizionariCondivisiAdminDizionari
I parametri da configurare sono:
* Admin action: create/modify

### DizionariCondivisiSpid
Da utilizzare nella pagina di login, mostra il pulsante che rimanda al gateway SPID.
Inserire nella cartella *Certificate* il file .pem

I parametri da configurare sono:
* URL: indicare l'URL del gateway SPID
* Servizio: indicare il nome del servizio per identificarsi nel gateway SPID

### DizionariCondivisiSpidApi
Inserire nella cartella *Certificate* il file .pem

#### Errori
Creare una pagina con URL *Errori*: qui saranno re-indirizzati gli utenti in caso di errori durante in login con SPID. Il codice da utilizzare (da aggiungere nel portale come RazorHost) è contenuto nel file *ViewPage1.cshtml*