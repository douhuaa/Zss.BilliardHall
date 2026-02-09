#!/usr/bin/env python3
"""
FindRepositoryRoot 自动化重构工具
使用正则表达式批量替换和删除方法
"""

import os
import re
import sys
from pathlib import Path
from typing import List, Tuple

# 配置
TEST_DIR = "src/tests/ArchitectureTests"
TARGET_NAMESPACE = "Zss.BilliardHall.Tests.ArchitectureTests.Shared"
REPLACEMENT_EXPR = "TestEnvironment.RepositoryRoot"

# 统计
stats = {
    'files_processed': 0,
    'files_modified': 0,
    'calls_replaced': 0,
    'methods_removed': 0,
    'modified_files': []
}

def find_cs_files(directory: str) -> List[Path]:
    """查找所有 C# 文件"""
    path = Path(directory)
    cs_files = []
    for file in path.rglob("*.cs"):
        if file.name != "TestEnvironment.cs":
            cs_files.append(file)
    return cs_files

def process_file(file_path: Path) -> bool:
    """处理单个文件，返回是否修改"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        file_modified = False
        
        # 1. 先删除 FindRepositoryRoot 方法定义（在替换调用之前）
        method_patterns = [
            # 模式1: 标准方法定义（完整的方法，包括辅助方法注释）
            r'\n    // =+ 辅助方法 =+\n\n    private static string\? FindRepositoryRoot\(\)(?:\s|\S)*?\n    \}',
            # 模式2: 无辅助方法注释的方法
            r'\n    private static string\? FindRepositoryRoot\(\)[^{]*\{(?:[^{}]|\{[^{}]*\})*\}',
            # 模式3: 带参数的版本
            r'\n    private static string\? FindRepositoryRoot\([^)]*\)[^{]*\{(?:[^{}]|\{[^{}]*\})*\}',
        ]
        
        methods_removed_in_file = 0
        for pattern in method_patterns:
            matches = re.findall(pattern, content, re.DOTALL)
            if matches:
                content = re.sub(pattern, '', content, flags=re.DOTALL)
                methods_removed_in_file += len(matches)
                stats['methods_removed'] += len(matches)
                file_modified = True
        
        # 2. 替换 FindRepositoryRoot() 调用（只在非方法声明的地方）
        call_pattern = r'FindRepositoryRoot\(\)'
        call_count = len(re.findall(call_pattern, content))
        
        if call_count > 0:
            print(f"处理: {file_path}")
            print(f"  找到 {call_count} 个 FindRepositoryRoot() 调用")
            
            content = re.sub(call_pattern, REPLACEMENT_EXPR, content)
            stats['calls_replaced'] += call_count
            
            if methods_removed_in_file > 0:
                print(f"  删除 {methods_removed_in_file} 个 FindRepositoryRoot 方法定义")
            
            file_modified = True
        
        # 3. 添加 using 声明（如果需要且不存在）
        if file_modified:
            using_pattern = rf'using {re.escape(TARGET_NAMESPACE)};'
            if not re.search(using_pattern, content):
                # 在最后一个 using 语句之后添加
                last_using_match = None
                for match in re.finditer(r'^using [^;]+;', content, re.MULTILINE):
                    last_using_match = match
                
                if last_using_match:
                    insert_pos = last_using_match.end()
                    content = (content[:insert_pos] + 
                             f"\nusing {TARGET_NAMESPACE};" + 
                             content[insert_pos:])
                    if call_count > 0:
                        print(f"  添加 using {TARGET_NAMESPACE}")
        
        # 4. 保存文件（如果有修改）
        if content != original_content:
            with open(file_path, 'w', encoding='utf-8', newline='\n') as f:
                f.write(content)
            
            stats['files_modified'] += 1
            stats['modified_files'].append(str(file_path))
            
            if call_count > 0:
                print(f"  ✓ 文件已更新\n")
            
            return True
        
        return False
        
    except Exception as e:
        print(f"  ✗ 错误: {e}\n")
        return False

def main():
    print("=" * 50)
    print("FindRepositoryRoot 自动化重构工具")
    print("=" * 50)
    print()
    
    # 查找所有 C# 文件
    cs_files = find_cs_files(TEST_DIR)
    print(f"找到 {len(cs_files)} 个 C# 文件待处理\n")
    
    # 处理每个文件
    for file_path in cs_files:
        stats['files_processed'] += 1
        process_file(file_path)
    
    # 输出统计
    print("=" * 50)
    print("重构完成统计")
    print("=" * 50)
    print(f"处理文件数: {stats['files_processed']}")
    print(f"修改文件数: {stats['files_modified']}")
    print(f"替换调用数: {stats['calls_replaced']}")
    print(f"删除方法数: {stats['methods_removed']}")
    print()
    
    if stats['modified_files']:
        print(f"修改了 {len(stats['modified_files'])} 个文件")
    
    print("\n重构工具执行完成！")

if __name__ == "__main__":
    main()
