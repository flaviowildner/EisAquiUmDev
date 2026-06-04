# Backend Plan

## Objetivo

Evoluir o backend atual de autenticacao para um backend de plataforma de cursos, com suporte a:

- catalogo de cursos
- modulos e aulas
- matricula
- progresso por aula
- administracao basica de conteudo

O foco inicial e MVP. Nada de pagamentos, comunidade ou analytics avancado neste momento.

## Milestone 1

Entregas da primeira fase:

1. PostgreSQL no lugar de `InMemory`
2. roles basicas no usuario
3. catalogo de cursos, modulos e aulas
4. matricula
5. progresso por aula
6. endpoints publicos e autenticados minimos

## Estrutura sugerida

Manter a estrutura simples por enquanto:

- `Application/Controllers`
- `Application/Data`
- `Application/DTOs`
- `Application/Models`
- `Application/Services`
- `Application/Repositories`

Arquivos provaveis:

- `Application/Models/Role.cs`
- `Application/Models/Category.cs`
- `Application/Models/Course.cs`
- `Application/Models/CourseModule.cs`
- `Application/Models/Lesson.cs`
- `Application/Models/Enrollment.cs`
- `Application/Models/LessonProgress.cs`
- `Application/Controllers/CoursesController.cs`
- `Application/Controllers/MeController.cs`
- `Application/Controllers/AdminCoursesController.cs`
- `Application/Services/CourseService.cs`
- `Application/Services/ProgressService.cs`

## Modelo de dominio

### User

- `Id`
- `Email`
- `GoogleId`
- `PasswordHash`
- `Name`
- `Role`
- `CreatedAt`

### Category

- `Id`
- `Name`
- `Slug`
- `Description`

### Course

- `Id`
- `CategoryId`
- `Title`
- `Slug`
- `ShortDescription`
- `Description`
- `Level`
- `ThumbnailUrl`
- `IsPublished`
- `CreatedAt`
- `UpdatedAt`

### CourseModule

- `Id`
- `CourseId`
- `Title`
- `Description`
- `Order`

### Lesson

- `Id`
- `CourseModuleId`
- `Title`
- `Slug`
- `Summary`
- `Content`
- `VideoUrl`
- `DurationInSeconds`
- `Order`
- `IsPreview`
- `IsPublished`

### Enrollment

- `Id`
- `UserId`
- `CourseId`
- `EnrolledAt`

### LessonProgress

- `Id`
- `UserId`
- `LessonId`
- `WatchedSeconds`
- `Completed`
- `LastAccessedAt`
- `CompletedAt`

## Relacionamentos

- `Category 1:N Course`
- `Course 1:N CourseModule`
- `CourseModule 1:N Lesson`
- `User 1:N Enrollment`
- `User 1:N LessonProgress`

## Regras de dominio

- curso tem muitos modulos
- modulo tem muitas aulas
- ordem de modulo e aula e explicita
- progresso pertence a `User + Lesson`
- matricula pertence a `User + Course`
- apenas cursos e aulas publicados aparecem publicamente
- aulas preview podem aparecer sem matricula
- slug de curso deve ser unico
- slug de categoria deve ser unico
- `Enrollment(UserId, CourseId)` deve ser unico
- `LessonProgress(UserId, LessonId)` deve ser unico

## Endpoints

### Publicos

- `GET /api/courses`
- `GET /api/courses/{slug}`
- `GET /api/categories`
- `GET /api/courses/{courseSlug}/lessons/{lessonSlug}`

### Autenticados

- `GET /api/me`
- `GET /api/me/courses`
- `POST /api/courses/{courseId}/enroll`
- `GET /api/me/courses/{courseId}/progress`
- `POST /api/me/lessons/{lessonId}/progress`
- `POST /api/me/lessons/{lessonId}/complete`

### Administrativos

- `POST /api/admin/courses`
- `PUT /api/admin/courses/{id}`
- `DELETE /api/admin/courses/{id}`
- `POST /api/admin/courses/{courseId}/modules`
- `PUT /api/admin/modules/{id}`
- `DELETE /api/admin/modules/{id}`
- `POST /api/admin/modules/{moduleId}/lessons`
- `PUT /api/admin/lessons/{id}`
- `DELETE /api/admin/lessons/{id}`

## DTOs sugeridos

- `CourseListItemDto`
- `CourseDetailsDto`
- `ModuleDto`
- `LessonDto`
- `EnrollRequestDto`
- `LessonProgressUpsertDto`

Os controllers nao devem expor entidades do EF diretamente.

## Banco

### Escolha

Usar PostgreSQL.

Motivos:

- melhor alinhamento com Railway
- persistencia real para cursos e progresso
- bom suporte relacional
- evolui melhor que `InMemory`

### Mudancas previstas

1. remover `UseInMemoryDatabase`
2. adicionar `UseNpgsql`
3. configurar connection string
4. criar migrations
5. aplicar migrations no ambiente

### Indices e constraints

- `Users.Email`
- `Categories.Slug`
- `Courses.Slug`
- `Lessons.Slug`
- `Enrollments(UserId, CourseId)` unico
- `LessonProgress(UserId, LessonId)` unico

## Backlog tecnico

### 1. Migrar banco para PostgreSQL

Objetivo:

- trocar `InMemory` por Postgres
- habilitar migrations

Pronto quando:

- API sobe com Postgres
- migrations criam o schema inicial

### 2. Evoluir User

Objetivo:

- adicionar role ao usuario
- preparar autorizacao por perfil

Pronto quando:

- JWT carrega role
- endpoints admin podem usar `[Authorize(Roles = "...")]`

### 3. Criar entidades de catalogo

Objetivo:

- modelar categorias, cursos, modulos e aulas

Pronto quando:

- DbContext mapeia tudo
- migration criada

### 4. Criar entidades de acesso do aluno

Objetivo:

- suportar matricula e progresso

Pronto quando:

- aluno pode se matricular
- progresso de aula pode ser persistido

### 5. Configurar constraints e indices

Objetivo:

- garantir integridade e evitar duplicidades

Pronto quando:

- schema possui unicidade e indices basicos

### 6. Criar DTOs

Objetivo:

- separar contrato da API das entidades

Pronto quando:

- controllers retornam DTOs

### 7. Implementar endpoints publicos

Objetivo:

- expor catalogo e detalhe de curso

Pronto quando:

- frontend consegue listar cursos
- detalhe de curso retorna modulos e aulas publicadas

### 8. Implementar endpoints do aluno

Objetivo:

- permitir matricula e progresso

Pronto quando:

- usuario se matricula
- progresso fica salvo e consultavel

### 9. Implementar endpoints admin

Objetivo:

- permitir cadastro de conteudo via API

Pronto quando:

- admin cria curso, modulo e aula sem mexer no banco manualmente

### 10. Adicionar autorizacao por role

Objetivo:

- separar aluno de admin

Pronto quando:

- aluno nao acessa endpoints admin
- admin acessa normalmente

## Ordem recomendada de implementacao

1. PostgreSQL
2. role no usuario
3. `Category`, `Course`, `CourseModule`, `Lesson`
4. endpoints publicos
5. `Enrollment`, `LessonProgress`
6. endpoints do aluno
7. endpoints admin
8. autorizacao por role
9. seeds iniciais
10. testes

## Fora do escopo agora

- pagamentos
- assinatura
- comentarios
- certificados
- recomendacoes
- upload nativo de video
- analytics detalhado

## Criterio de MVP backend pronto

O backend da plataforma pode ser considerado pronto para a primeira fase quando:

- usuario consegue autenticar
- usuario consegue listar cursos
- usuario consegue abrir o detalhe do curso
- usuario consegue se matricular
- usuario consegue registrar progresso
- admin consegue cadastrar curso, modulo e aula
- tudo persiste em PostgreSQL
