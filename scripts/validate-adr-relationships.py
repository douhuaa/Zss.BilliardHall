#!/usr/bin/env python3
"""
ADR 关系一致性验证器
验证 ADR 之间的双向关系是否一致
"""

import re
import sys
from pathlib import Path
from typing import Dict, List, Set
from collections import defaultdict

# ANSI 颜色代码
RED = '\033[0;31m'
YELLOW = '\033[1;33m'
GREEN = '\033[0;32m'
BLUE = '\033[0;34m'
NC = '\033[0m'  # No Color


class ADRRelationship:
    """ADR 关系数据类"""
    def __init__(self, adr_id: str):
        self.adr_id = adr_id
        self.depends_on: Set[str] = set()
        self.depended_by: Set[str] = set()
        self.supersedes: Set[str] = set()
        self.superseded_by: Set[str] = set()
        self.related: Set[str] = set()
        self.inherits: Set[str] = set()
        self.inherited_by: Set[str] = set()


def extract_adr_id(filename: str) -> str:
    """从文件名提取 ADR 编号"""
    match = re.search(r'ADR-(\d+)', filename)
    if match:
        return match.group(1)
    return ""


def extract_relationships(adr_file: Path) -> ADRRelationship:
    """提取 ADR 文件中的关系声明"""
    try:
        content = adr_file.read_text(encoding='utf-8')
    except Exception as e:
        print(f"{YELLOW}⚠️  无法读取文件 {adr_file}: {e}{NC}")
        return None
    
    adr_id = extract_adr_id(adr_file.name)
    if not adr_id:
        return None
    
    rel = ADRRelationship(adr_id)
    
    # 提取关系声明部分
    # 寻找 "## ADR 关系" 或 "## 关系" 章节
    relationship_section = ""
    in_relationship = False
    
    for line in content.split('\n'):
        if re.match(r'^##\s+(ADR\s*)?关系', line) or re.match(r'^##\s+Relationships', line):
            in_relationship = True
            continue
        elif in_relationship and re.match(r'^##\s+', line):
            break
        elif in_relationship:
            relationship_section += line + '\n'
    
    if not relationship_section:
        # 尝试查找 Front Matter 中的关系
        front_matter_match = re.search(r'^---\n(.*?)\n---', content, re.DOTALL | re.MULTILINE)
        if front_matter_match:
            front_matter = front_matter_match.group(1)
            # 提取 supersedes 和 superseded_by
            for line in front_matter.split('\n'):
                if line.startswith('supersedes:'):
                    value = line.split(':', 1)[1].strip()
                    if value and value != 'null':
                        rel.supersedes.add(extract_adr_id(value))
                elif line.startswith('superseded_by:'):
                    value = line.split(':', 1)[1].strip()
                    if value and value != 'null':
                        rel.superseded_by.add(extract_adr_id(value))
    
    # 解析关系声明
    # 新的解析方法：逐段解析，支持多行格式
    if relationship_section:
        # 分割成不同的关系类型段落
        rel_types_map = {
            '依赖': 'depends_on',
            'Depends On': 'depends_on',
            '被依赖': 'depended_by',
            'Depended By': 'depended_by',
            '替代': 'supersedes',
            'Supersedes': 'supersedes',
            '被替代': 'superseded_by',
            'Superseded By': 'superseded_by',
            '相关': 'related',
            'Related': 'related',
            '继承': 'inherits',
            'Inherits': 'inherits',
            '被继承': 'inherited_by',
            'Inherited By': 'inherited_by',
        }
        
        current_rel_type = None
        for line in relationship_section.split('\n'):
            # 检查是否是关系类型标题
            found_rel_type = False
            for cn_name, en_name in rel_types_map.items():
                if re.match(rf'^\*\*{re.escape(cn_name)}', line):
                    current_rel_type = en_name
                    found_rel_type = True
                    # 尝试在同一行找到 ADR 引用
                    adrs = re.findall(r'ADR-(\d+)', line)
                    for adr in adrs:
                        getattr(rel, current_rel_type).add(adr)
                    break
            
            if not found_rel_type:
                # 如果在某个关系类型段落中，查找 ADR 引用
                if current_rel_type and line.strip().startswith('-'):
                    adrs = re.findall(r'ADR-(\d+)', line)
                    for adr in adrs:
                        getattr(rel, current_rel_type).add(adr)
                # 非空行且不是列表项且不是加粗标记结束当前段落
                elif line.strip() and not line.strip().startswith('-') and not line.strip().startswith('*'):
                    if current_rel_type:
                        current_rel_type = None
    
    return rel


def validate_bidirectional(adr_map: Dict[str, ADRRelationship]) -> List[str]:
    """验证双向关系一致性"""
    errors = []
    
    for adr_id, rel in adr_map.items():
        # 检查依赖关系
        for dep in rel.depends_on:
            if dep in adr_map:
                if adr_id not in adr_map[dep].depended_by:
                    errors.append(
                        f"{RED}❌ ADR-{adr_id} 声明依赖 ADR-{dep}，"
                        f"但 ADR-{dep} 未声明被 ADR-{adr_id} 依赖{NC}"
                    )
        
        # 检查被依赖关系
        for dep_by in rel.depended_by:
            if dep_by in adr_map:
                if adr_id not in adr_map[dep_by].depends_on:
                    errors.append(
                        f"{RED}❌ ADR-{adr_id} 声明被 ADR-{dep_by} 依赖，"
                        f"但 ADR-{dep_by} 未声明依赖 ADR-{adr_id}{NC}"
                    )
        
        # 检查替代关系
        for sup in rel.supersedes:
            if sup in adr_map:
                if adr_id not in adr_map[sup].superseded_by:
                    errors.append(
                        f"{RED}❌ ADR-{adr_id} 声明替代 ADR-{sup}，"
                        f"但 ADR-{sup} 未声明被 ADR-{adr_id} 替代{NC}"
                    )
        
        # 检查被替代关系
        for sup_by in rel.superseded_by:
            if sup_by in adr_map:
                if adr_id not in adr_map[sup_by].supersedes:
                    errors.append(
                        f"{RED}❌ ADR-{adr_id} 声明被 ADR-{sup_by} 替代，"
                        f"但 ADR-{sup_by} 未声明替代 ADR-{adr_id}{NC}"
                    )
        
        # 检查继承关系
        for inh in rel.inherits:
            if inh in adr_map:
                if adr_id not in adr_map[inh].inherited_by:
                    errors.append(
                        f"{YELLOW}⚠️  ADR-{adr_id} 声明继承 ADR-{inh}，"
                        f"但 ADR-{inh} 未声明被 ADR-{adr_id} 继承{NC}"
                    )
    
    return errors


def check_circular_dependencies(adr_map: Dict[str, ADRRelationship]) -> List[str]:
    """检查循环依赖"""
    warnings = []
    
    def find_cycle(start: str, current: str, visited: Set[str], path: List[str]) -> bool:
        if current in visited:
            if current == start and len(path) > 1:
                cycle_str = " → ".join([f"ADR-{x}" for x in path + [current]])
                warnings.append(
                    f"{YELLOW}⚠️  检测到循环依赖：{cycle_str}{NC}"
                )
                return True
            return False
        
        visited.add(current)
        path.append(current)
        
        if current in adr_map:
            for dep in adr_map[current].depends_on:
                if find_cycle(start, dep, visited.copy(), path.copy()):
                    return True
        
        return False
    
    for adr_id in adr_map:
        find_cycle(adr_id, adr_id, set(), [])
    
    return warnings


def check_orphaned_relationships(adr_map: Dict[str, ADRRelationship]) -> List[str]:
    """检查孤立的关系声明（引用不存在的 ADR）"""
    warnings = []
    
    all_adr_ids = set(adr_map.keys())
    
    for adr_id, rel in adr_map.items():
        all_refs = (rel.depends_on | rel.depended_by | rel.supersedes | 
                   rel.superseded_by | rel.related | rel.inherits | rel.inherited_by)
        
        for ref in all_refs:
            if ref not in all_adr_ids:
                warnings.append(
                    f"{YELLOW}⚠️  ADR-{adr_id} 引用了不存在的 ADR-{ref}{NC}"
                )
    
    return warnings


def generate_statistics(adr_map: Dict[str, ADRRelationship]):
    """生成统计信息"""
    print(f"\n{BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━{NC}")
    print(f"{BLUE}统计信息{NC}")
    print(f"{BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━{NC}\n")
    
    total_adrs = len(adr_map)
    total_dependencies = sum(len(rel.depends_on) for rel in adr_map.values())
    total_supersedes = sum(len(rel.supersedes) for rel in adr_map.values())
    
    adrs_with_no_relations = sum(
        1 for rel in adr_map.values() 
        if not (rel.depends_on or rel.depended_by or rel.supersedes or 
                rel.superseded_by or rel.related or rel.inherits or rel.inherited_by)
    )
    
    print(f"ADR 总数：{total_adrs}")
    print(f"依赖关系总数：{total_dependencies}")
    print(f"替代关系总数：{total_supersedes}")
    print(f"无关系声明的 ADR：{adrs_with_no_relations}")
    print()


def main():
    """主函数"""
    print(f"{BLUE}🔍 开始 ADR 关系一致性验证...{NC}\n")
    
    adr_dir = Path('docs/adr')
    if not adr_dir.exists():
        print(f"{RED}❌ 错误：找不到 ADR 目录 {adr_dir}{NC}")
        sys.exit(1)
    
    # 扫描所有 ADR
    adr_map: Dict[str, ADRRelationship] = {}
    
    print(f"{BLUE}扫描 ADR 文件...{NC}")
    for adr_file in adr_dir.rglob('ADR-*.md'):
        rel = extract_relationships(adr_file)
        if rel:
            adr_map[rel.adr_id] = rel
            print(f"  • 已解析 ADR-{rel.adr_id}")
    
    print(f"\n{GREEN}✅ 成功解析 {len(adr_map)} 个 ADR{NC}\n")
    
    # 验证双向关系一致性
    print(f"{BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━{NC}")
    print(f"{BLUE}检查 1: 双向关系一致性{NC}")
    print(f"{BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━{NC}\n")
    
    errors = validate_bidirectional(adr_map)
    
    if not errors:
        print(f"{GREEN}✅ 所有双向关系声明一致{NC}\n")
    else:
        print(f"{RED}发现 {len(errors)} 个关系不一致问题：{NC}\n")
        for error in errors:
            print(f"  {error}")
        print()
    
    # 检查循环依赖
    print(f"{BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━{NC}")
    print(f"{BLUE}检查 2: 循环依赖{NC}")
    print(f"{BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━{NC}\n")
    
    cycle_warnings = check_circular_dependencies(adr_map)
    
    if not cycle_warnings:
        print(f"{GREEN}✅ 未检测到循环依赖{NC}\n")
    else:
        print(f"{YELLOW}发现 {len(cycle_warnings)} 个潜在循环：{NC}\n")
        for warning in cycle_warnings:
            print(f"  {warning}")
        print()
    
    # 检查孤立关系
    print(f"{BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━{NC}")
    print(f"{BLUE}检查 3: 孤立关系引用{NC}")
    print(f"{BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━{NC}\n")
    
    orphan_warnings = check_orphaned_relationships(adr_map)
    
    if not orphan_warnings:
        print(f"{GREEN}✅ 所有关系引用都有效{NC}\n")
    else:
        print(f"{YELLOW}发现 {len(orphan_warnings)} 个孤立引用：{NC}\n")
        for warning in orphan_warnings:
            print(f"  {warning}")
        print()
    
    # 生成统计信息
    generate_statistics(adr_map)
    
    # 总结
    print(f"{BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━{NC}")
    print(f"{BLUE}验证总结{NC}")
    print(f"{BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━{NC}\n")
    
    total_issues = len(errors)
    total_warnings = len(cycle_warnings) + len(orphan_warnings)
    
    if total_issues == 0 and total_warnings == 0:
        print(f"{GREEN}✅ ADR 关系验证通过！未发现问题。{NC}")
        return 0
    else:
        if total_issues > 0:
            print(f"{RED}❌ 发现 {total_issues} 个严重问题需要修复{NC}")
        if total_warnings > 0:
            print(f"{YELLOW}⚠️  发现 {total_warnings} 个警告需要关注{NC}")
        print(f"\n{YELLOW}📋 请参阅 docs/reports/adr-synchronization-analysis-2026-01-29.md 了解详细整改建议。{NC}")
        return 1 if total_issues > 0 else 0


if __name__ == '__main__':
    sys.exit(main())
