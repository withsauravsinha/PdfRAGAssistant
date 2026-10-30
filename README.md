# PDF Knowledge Assistant – RAG Application

A Retrieval-Augmented Generation (RAG) application built using **C#, ASP.NET Core, Ollama, PostgreSQL, and pgvector**.

This application allows users to upload PDF documents, extract their content, generate vector embeddings, store them in PostgreSQL, and ask questions based on uploaded documents.

The application uses locally hosted AI models through Ollama. No OpenAI API key or cloud-based LLM API is required.

## 1. Technologies Used

| Technology           | Purpose                              |
| -------------------- | ------------------------------------ |
| C#                   | Backend programming language         |
| ASP.NET Core Web API | REST API development                 |
| PdfPig               | PDF text extraction                  |
| Ollama               | Local AI model execution             |
| nomic-embed-text     | Text embedding generation            |
| llama3.2             | Answer generation                    |
| PostgreSQL           | Database                             |
| pgvector             | Vector storage and similarity search |
| Npgsql               | PostgreSQL connectivity              |

## 2. Prerequisites

Install the following software before setting up the project.

### Required Software

1. **Visual Studio 2022**
   - Install ASP.NET and web development workload.
   - Install .NET SDK compatible with the project.

2. **PostgreSQL**
   - Install PostgreSQL.
   - Remember your database username and password.
   - Verify that PostgreSQL is running.

3. **Ollama**
   - Download and install Ollama.
   - Install the required models:

```powershell
ollama pull nomic-embed-text
ollama pull llama3.2
```

4. **Git**
   - Required to clone the pgvector repository.

5. **Visual Studio C++ Build Tools**
   - Open Visual Studio Installer.
   - Select your Visual Studio 2022 installation and click Modify.
   - Select the **Desktop development with C++** workload.
   - Install the workload and its required components.

   This installation provides the Windows C++ build tools, including `nmake`, required to build pgvector from source.

### Important: Install pgvector on Windows

If pgvector is not already installed for your PostgreSQL installation, follow these steps.

**Step 1: Open the x64 Native Tools Command Prompt**

After installing Desktop development with C++, open:

Start Menu → Visual Studio 2022 → x64 Native Tools Command Prompt for VS 2022.

This command prompt configures the required Visual Studio C++ build environment.

**Step 2: Verify nmake**

Execute:

```cmd
nmake /?
```

If the command is not recognized, make sure the Desktop development with C++ workload is installed and reopen the x64 Native Tools Command Prompt.

**Step 3: Download pgvector**

Clone the official repository:

```cmd
git clone https://github.com/pgvector/pgvector.git
```

Navigate into the repository:

```cmd
cd pgvector
```

**Step 4: Configure the PostgreSQL installation path**

Set `PGROOT` to your actual PostgreSQL installation directory.

Example:

```cmd
set "PGROOT=C:\Program Files\PostgreSQL\17"
```

Replace `17` with your installed PostgreSQL version.

**Step 5: Build and install pgvector**

Run:

```cmd
nmake /F Makefile.win
```

After the build completes, open the x64 Native Tools Command Prompt as Administrator and run the installation command:

```cmd
nmake /F Makefile.win install
```

Make sure `PGROOT` points to the PostgreSQL installation where you want pgvector installed.

**Step 6: Restart PostgreSQL**

1. Press `Windows + R`.
2. Type `services.msc`.
3. Find the PostgreSQL service.
4. Right-click the service.
5. Select Restart.

**Step 7: Enable the pgvector extension**

Open pgAdmin and execute:

```sql
CREATE EXTENSION IF NOT EXISTS vector;
```

Verify that the extension is installed:

```sql
SELECT * FROM pg_extension WHERE extname = 'vector';
```

If the extension is returned successfully, pgvector is ready.

Official pgvector repository and installation instructions:

https://github.com/pgvector/pgvector

## 3. Ollama Setup

Check the installed models:

```powershell
ollama list
```

Required models:

| Model            | Purpose                              |
| ---------------- | ------------------------------------ |
| nomic-embed-text | Converts text into vector embeddings |
| llama3.2         | Generates natural-language answers   |

Ollama runs locally at:

```text
http://localhost:11434
```

Important API endpoints:

| Endpoint     | Purpose                       |
| ------------ | ----------------------------- |
| `/api/embed` | Generate text embeddings      |
| `/api/chat`  | Generate answers using an LLM |

## 4. Architecture

```text
                  PDF DOCUMENT
                       |
                       v
               PDF TEXT EXTRACTION
                    (PdfPig)
                       |
                       v
                 TEXT CHUNKING
                       |
                       v
               OLLAMA EMBEDDINGS
                nomic-embed-text
                       |
                       v
              POSTGRESQL + PGVECTOR
                 Store chunks
                Store embeddings
                       |
                       |
              USER ASKS QUESTION
                       |
                       v
               QUESTION EMBEDDING
                nomic-embed-text
                       |
                       v
                SEMANTIC SEARCH
                 PostgreSQL
                       |
                       v
              RETRIEVE TOP 3 CHUNKS
                       |
                       v
                CONTEXT + QUESTION
                       |
                       v
                OLLAMA LLM
                  llama3.2
                       |
                       v
                  FINAL ANSWER
```

## 5. How RAG Works

The application has two main workflows.

### A. Document Ingestion

When a PDF is uploaded:

1. Extract text from the PDF using PdfPig.
2. Split the extracted text into smaller chunks.
3. Send chunks to Ollama's embedding model.
4. Generate a vector embedding for each chunk.
5. Store the original text and corresponding embeddings in PostgreSQL.

### B. Question Answering

When a user asks a question:

1. Receive the user's question through the API.
2. Generate an embedding for the question using Ollama.
3. Search PostgreSQL using pgvector cosine distance.
4. Retrieve the most relevant document chunks.
5. Send the question and retrieved context to Ollama's LLM.
6. Generate an answer based on the retrieved document content.
7. Return the answer through the API.

## 6. PostgreSQL Setup

Enable the pgvector extension:

```sql
CREATE EXTENSION IF NOT EXISTS vector;
```

Create the document chunks table:

```sql
CREATE TABLE document_chunks (
    id BIGSERIAL PRIMARY KEY,
    document_name TEXT,
    chunk_index INT,
    content TEXT,
    embedding VECTOR(768)
);
```

The embedding dimension is based on the configured embedding model.

### Database Structure

| Column        | Description              |
| ------------- | ------------------------ |
| id            | Unique record identifier |
| document_name | Uploaded PDF filename    |
| chunk_index   | Chunk sequence number    |
| content       | Extracted text chunk     |
| embedding     | 768-dimensional vector   |

## 7. Semantic Search

The application uses pgvector's cosine distance operator:

```sql
SELECT
    content,
    embedding <=> $1::vector AS distance
FROM document_chunks
ORDER BY embedding <=> $1::vector
LIMIT 3;
```

The `<=>` operator calculates cosine distance.

A smaller distance indicates greater similarity.

## 8. Project Structure

```text
PdfKnowledgeAssistant/
│
├── Controllers/
│   └── PdfController.cs
│
├── Services/
│   ├── EmbeddingService.cs
│   ├── VectorStorageService.cs
│   ├── SemanticSearchService.cs
│   └── LlmService.cs
│
├── Documents/
│   └── superman.pdf
│
├── Program.cs
├── appsettings.json
└── README.md
```

## 9. Configuration

Configure the PostgreSQL connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "PostgresConnection": "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

Configure Ollama's HTTP clients in `Program.cs`:

```csharp
builder.Services.AddHttpClient<EmbeddingService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:11434/");
    client.Timeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddHttpClient<LlmService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:11434/");
    client.Timeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddScoped<VectorStorageService>();
builder.Services.AddScoped<SemanticSearchService>();
```

## 10. API Endpoints

| Endpoint          | Method | Purpose                  |
| ----------------- | ------ | ------------------------ |
| `/api/pdf/upload` | POST   | Upload and process a PDF |
| `/api/pdf/search` | GET    | Perform semantic search  |
| `/api/pdf/ask`    | GET    | Ask questions using RAG  |

### Example: Semantic Search

```http
GET /api/pdf/search?question=Where%20was%20Superman%20born?
```

Returns relevant document chunks.

### Example: Ask a Question

```http
GET /api/pdf/ask?question=Where%20was%20Superman%20born?
```

Sample response:

```json
{
  "question": "Where was Superman born?",
  "answer": "Superman was born on the planet Krypton.",
  "retrievedChunks": 3
}
```

## 11. Running the Application

Follow these steps after completing the prerequisites:

1. Start PostgreSQL.
2. Start Ollama.
3. Verify that both Ollama models are installed.
4. Configure the database connection string.
5. Ensure pgvector is installed and the extension is enabled.
6. Ensure the `document_chunks` table exists.
7. Run the ASP.NET Core application.
8. Open Swagger.
9. Upload a PDF document.
10. Generate and store embeddings.
11. Ask questions through the RAG endpoint.

## 12. Example Questions

Using the sample Superman PDF:

- Where was Superman born?
- What is Superman's Kryptonian birth name?
- Who are his biological parents?
- Who raised Superman?
- What is his secret identity?
- What are Superman's superpowers?
- What is Kryptonite?
- Where does Clark Kent work?

The application retrieves relevant chunks and generates answers using the locally hosted LLM.

## 13. Future Improvements

Potential enhancements:

- Support multiple PDF documents.
- Add source citations and page numbers.
- Implement similarity thresholds.
- Add vector indexing using HNSW or IVFFlat.
- Support conversation history.
- Implement document deletion and re-indexing.
- Add authentication and authorization.
- Improve chunking with token-based splitting.
- Add metadata filtering.
- Add streaming LLM responses.

## 14. Key Learning Concepts

This project demonstrates:

- Retrieval-Augmented Generation (RAG)
- Text extraction and preprocessing
- Text chunking
- Vector embeddings
- Vector databases
- Semantic similarity search
- Cosine distance
- Context augmentation
- Large Language Models (LLMs)
- Local AI model integration

---

**Project:** PDF Knowledge Assistant

**Architecture:** Retrieval-Augmented Generation (RAG)

**Backend:** ASP.NET Core Web API

**AI Runtime:** Ollama

**Database:** PostgreSQL + pgvector

**Status:** Working end-to-end RAG prototype.
