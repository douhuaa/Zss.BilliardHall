#!/usr/bin/env python3
"""
JSON Schema 验证脚本 (JSON Schema Validation Script)

验证 .copilot/schemas 目录下所有 JSON Schema 文件的正确性
Validates all JSON Schema files in the .copilot/schemas directory
"""

import json
import os
import sys
from pathlib import Path

def validate_json_schema(file_path):
    """验证单个 JSON Schema 文件"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            schema = json.load(f)
        
        # 基本检查
        required_fields = ['$schema', 'title', 'description']
        missing_fields = [field for field in required_fields if field not in schema]
        
        if missing_fields:
            return False, f"缺少必需字段: {', '.join(missing_fields)}"
        
        # 检查 $schema 字段是否为有效的 JSON Schema URL
        if not schema['$schema'].startswith('http://json-schema.org/'):
            return False, f"无效的 $schema 值: {schema['$schema']}"
        
        return True, "验证通过"
        
    except json.JSONDecodeError as e:
        return False, f"JSON 格式错误: {str(e)}"
    except Exception as e:
        return False, f"验证失败: {str(e)}"

def main():
    """主验证函数"""
    schemas_dir = Path(__file__).parent / 'schemas'
    
    if not schemas_dir.exists():
        print(f"❌ 错误: schemas 目录不存在: {schemas_dir}")
        sys.exit(1)
    
    print("🔍 开始验证 JSON Schema 文件...")
    print(f"📁 目录: {schemas_dir}")
    print("-" * 50)
    
    json_files = list(schemas_dir.glob('*.json'))
    
    if not json_files:
        print("⚠️  警告: 未找到 JSON 文件")
        sys.exit(0)
    
    all_valid = True
    
    for json_file in json_files:
        is_valid, message = validate_json_schema(json_file)
        
        status = "✅" if is_valid else "❌"
        print(f"{status} {json_file.name}: {message}")
        
        if not is_valid:
            all_valid = False
    
    print("-" * 50)
    
    if all_valid:
        print(f"🎉 所有 {len(json_files)} 个 Schema 文件验证通过!")
        sys.exit(0)
    else:
        print("💥 存在无效的 Schema 文件，请修复后重新验证")
        sys.exit(1)

if __name__ == '__main__':
    main()