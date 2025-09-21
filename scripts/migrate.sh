#!/bin/bash
# 迁移脚本

set -e

echo "🗄️  Running database migrations..."

# 检查是否有docker compose服务运行
if ! docker compose ps mysql | grep -q "Up"; then
    echo "启动MySQL服务..."
    docker compose up -d mysql
    
    # 等待MySQL启动
    echo "等待MySQL启动..."
    sleep 10
fi

# 检查数据库连接
echo "检查数据库连接..."
docker compose exec mysql mysql -ubilliard -pbilliard123 -e "SELECT 1;" billiard_hall

# 运行迁移（当前阶段使用SQL文件）
echo "✅ 数据库连接正常"
echo "Schema已通过docker-entrypoint-initdb.d自动加载"

# 如果需要重新初始化schema
if [ "$1" = "--reset" ]; then
    echo "⚠️  重置数据库..."
    docker compose exec mysql mysql -ubilliard -pbilliard123 -e "DROP DATABASE IF EXISTS billiard_hall; CREATE DATABASE billiard_hall;" 
    docker compose exec mysql mysql -ubilliard -pbilliard123 billiard_hall < db/schema.sql
    docker compose exec mysql mysql -ubilliard -pbilliard123 billiard_hall < db/seed-data.sql
    echo "✅ 数据库重置完成"
fi

echo "🎉 迁移完成"