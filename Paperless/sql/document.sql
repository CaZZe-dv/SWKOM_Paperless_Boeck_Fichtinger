CREATE TABLE document (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    content BYTEA NOT NULL,
    uploadDate TIMESTAMP NOT NULL
);
