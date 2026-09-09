-- SentinelQA 0001: extensions, schemas, identity / tenants / network
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

CREATE SCHEMA IF NOT EXISTS identity;
CREATE SCHEMA IF NOT EXISTS tenants;
CREATE SCHEMA IF NOT EXISTS network;
CREATE SCHEMA IF NOT EXISTS policy;
CREATE SCHEMA IF NOT EXISTS change_management;
CREATE SCHEMA IF NOT EXISTS qa;
CREATE SCHEMA IF NOT EXISTS defects;
CREATE SCHEMA IF NOT EXISTS notifications;
CREATE SCHEMA IF NOT EXISTS audit;

-- ── Tenants ────────────────────────────────────────────────────────────────
CREATE TABLE tenants.tenants (
    id         uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    name       text NOT NULL,
    slug       text NOT NULL CONSTRAINT uq_tenants_slug UNIQUE,
    is_active  boolean NOT NULL DEFAULT true,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE tenants.teams (
    id         uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id  uuid NOT NULL REFERENCES tenants.tenants(id) ON DELETE CASCADE,
    name       text NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_teams_name_per_tenant UNIQUE (tenant_id, name)
);

-- ── Identity ───────────────────────────────────────────────────────────────
CREATE TABLE identity.users (
    id            uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id     uuid NOT NULL REFERENCES tenants.tenants(id),
    email         text NOT NULL CONSTRAINT uq_users_email UNIQUE,
    display_name  text NOT NULL,
    password_hash text NOT NULL,
    is_active     boolean NOT NULL DEFAULT true,
    last_login_at timestamptz,
    created_at    timestamptz NOT NULL DEFAULT now(),
    updated_at    timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE identity.roles (
    id          uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    name        text NOT NULL CONSTRAINT uq_roles_name UNIQUE,
    description text
);

CREATE TABLE identity.permissions (
    id   uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    name text NOT NULL CONSTRAINT uq_permissions_name UNIQUE
);

CREATE TABLE identity.user_roles (
    user_id uuid NOT NULL REFERENCES identity.users(id) ON DELETE CASCADE,
    role_id uuid NOT NULL REFERENCES identity.roles(id) ON DELETE CASCADE,
    PRIMARY KEY (user_id, role_id)
);

CREATE TABLE identity.role_permissions (
    role_id       uuid NOT NULL REFERENCES identity.roles(id) ON DELETE CASCADE,
    permission_id uuid NOT NULL REFERENCES identity.permissions(id) ON DELETE CASCADE,
    PRIMARY KEY (role_id, permission_id)
);

CREATE TABLE identity.refresh_tokens (
    id                     uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id                uuid NOT NULL REFERENCES identity.users(id) ON DELETE CASCADE,
    token_hash             text NOT NULL CONSTRAINT uq_refresh_token_hash UNIQUE,
    issued_at              timestamptz NOT NULL DEFAULT now(),
    expires_at             timestamptz NOT NULL,
    revoked_at             timestamptz,
    replaced_by_token_hash text,
    created_by_ip          text
);

CREATE TABLE tenants.team_members (
    team_id uuid NOT NULL REFERENCES tenants.teams(id) ON DELETE CASCADE,
    user_id uuid NOT NULL REFERENCES identity.users(id) ON DELETE CASCADE,
    PRIMARY KEY (team_id, user_id)
);

-- ── Network ────────────────────────────────────────────────────────────────
CREATE TABLE network.firewalls (
    id          uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id   uuid NOT NULL REFERENCES tenants.tenants(id),
    name        text NOT NULL,
    vendor      text NOT NULL,
    environment text NOT NULL CHECK (environment IN ('Development','QA','Staging','Production')),
    status      text NOT NULL DEFAULT 'Unknown' CHECK (status IN ('Online','Offline','Degraded','Unknown')),
    description text,
    created_at  timestamptz NOT NULL DEFAULT now(),
    updated_at  timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_firewalls_name_per_tenant UNIQUE (tenant_id, name, environment)
);

CREATE TABLE network.networks (
    id          uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id   uuid NOT NULL REFERENCES tenants.tenants(id),
    name        text NOT NULL,
    cidr        text NOT NULL,
    environment text NOT NULL CHECK (environment IN ('Development','QA','Staging','Production')),
    description text,
    created_at  timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_networks_name_per_tenant UNIQUE (tenant_id, name)
);

CREATE TABLE network.network_groups (
    id        uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id uuid NOT NULL REFERENCES tenants.tenants(id),
    name      text NOT NULL,
    CONSTRAINT uq_network_groups_name UNIQUE (tenant_id, name)
);

CREATE TABLE network.network_group_members (
    network_group_id uuid NOT NULL REFERENCES network.network_groups(id) ON DELETE CASCADE,
    network_id       uuid NOT NULL REFERENCES network.networks(id) ON DELETE CASCADE,
    PRIMARY KEY (network_group_id, network_id)
);

CREATE TABLE network.services (
    id        uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id uuid NOT NULL REFERENCES tenants.tenants(id),
    name      text NOT NULL,
    protocol  text NOT NULL CHECK (protocol IN ('TCP','UDP','ICMP','Any')),
    port_start int NOT NULL CHECK (port_start BETWEEN 0 AND 65535),
    port_end   int NOT NULL CHECK (port_end BETWEEN 0 AND 65535),
    CONSTRAINT uq_services_name UNIQUE (tenant_id, name),
    CONSTRAINT chk_services_ports CHECK (port_start <= port_end)
);

CREATE TABLE network.service_groups (
    id        uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id uuid NOT NULL REFERENCES tenants.tenants(id),
    name      text NOT NULL,
    CONSTRAINT uq_service_groups_name UNIQUE (tenant_id, name)
);

CREATE TABLE network.service_group_members (
    service_group_id uuid NOT NULL REFERENCES network.service_groups(id) ON DELETE CASCADE,
    service_id       uuid NOT NULL REFERENCES network.services(id) ON DELETE CASCADE,
    PRIMARY KEY (service_group_id, service_id)
);