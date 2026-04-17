/// <reference types="vitest" />
import { fileURLToPath, URL } from 'node:url';
import { defineConfig, type UserConfig } from 'vite';
import type { UserConfig as VitestUserConfig } from 'vitest/config';
import plugin from '@vitejs/plugin-react';
import fs from 'fs';
import path from 'path';
import child_process from 'child_process';
import { env } from 'process';

const baseFolder =
    env.APPDATA && env.APPDATA !== ''
        ? `${env.APPDATA}/ASP.NET/https`
        : `${env.HOME}/.aspnet/https`;

const certificateName = 'novacrm.client';
const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

const target =
    env.ASPNETCORE_HTTPS_PORT
        ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`
        : (env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:7226');

export default defineConfig(({ command }) => {
    const isVitest = env.VITEST === 'true';
    const isDevServer = command === 'serve';

    let httpsConfig: { key: Buffer; cert: Buffer } | undefined;
    if (isDevServer && !isVitest)
    {
        if (!fs.existsSync(baseFolder)) {
            fs.mkdirSync(baseFolder, { recursive: true });
        }

        if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
            const res = child_process.spawnSync(
                'dotnet',
                ['dev-certs', 'https', '--export-path', certFilePath, '--format', 'Pem', '--no-password'],
                { stdio: 'inherit' }
            );
            if (res.status !== 0) throw new Error('Could not create certificate.');
        }

        httpsConfig = {
            key: fs.readFileSync(keyFilePath),
            cert: fs.readFileSync(certFilePath),
        };
    }

    const config: UserConfig & { test: VitestUserConfig['test'] } = {
        plugins: [plugin()],
        resolve: {
            alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) }
        },
        test: {
            environment: 'jsdom',
            setupFiles: ['./src/setupTests.ts'],
            css: true,
        },
        server: {
            port: parseInt(env.DEV_SERVER_PORT || '58876'),
            https: httpsConfig,
            proxy: {
                '^/api': {
                    target,
                    changeOrigin: true,
                    secure: false
                }
            }
        }
    };

    return config;
});
