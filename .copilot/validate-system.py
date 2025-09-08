#!/usr/bin/env python3
"""
GitHub Copilot 指令文件系统验证脚本
验证所有指令文件的完整性和有效性
"""

import os
import json
import yaml
import re
from pathlib import Path
from typing import List, Dict, Any

class CopilotSystemValidator:
    def __init__(self, base_path: str = "."):
        self.base_path = Path(base_path)
        self.errors: List[str] = []
        self.warnings: List[str] = []
        
    def validate_all(self) -> Dict[str, Any]:
        """验证整个 Copilot 指令系统"""
        print("🔍 开始验证 GitHub Copilot 指令文件系统...")
        
        # 验证目录结构
        self.validate_directory_structure()
        
        # 验证主要指令文件
        self.validate_main_instructions()
        
        # 验证配置文件
        self.validate_configuration()
        
        # 验证 JSON Schema 文件
        self.validate_schemas()
        
        # 验证模式文件
        self.validate_patterns()
        
        # 验证模板文件
        self.validate_templates()
        
        # 生成报告
        return self.generate_report()
    
    def validate_directory_structure(self):
        """验证目录结构"""
        print("📁 验证目录结构...")
        
        required_dirs = [
            ".github",
            ".copilot",
            ".copilot/schemas",
            ".copilot/patterns", 
            ".copilot/workflows",
            ".copilot/templates"
        ]
        
        for dir_path in required_dirs:
            full_path = self.base_path / dir_path
            if not full_path.exists():
                self.errors.append(f"缺少必需目录: {dir_path}")
            elif not full_path.is_dir():
                self.errors.append(f"路径不是目录: {dir_path}")
    
    def validate_main_instructions(self):
        """验证主要指令文件"""
        print("📋 验证主要指令文件...")
        
        main_file = self.base_path / ".github" / "copilot-instructions.md"
        if not main_file.exists():
            self.errors.append("缺少主要指令文件: .github/copilot-instructions.md")
            return
            
        content = main_file.read_text(encoding='utf-8')
        
        # 检查必要的节
        required_sections = [
            "项目概述",
            "技术栈", 
            "业务领域模型",
            "代码生成指南",
            "测试策略",
            "错误处理模式"
        ]
        
        for section in required_sections:
            if section not in content:
                self.warnings.append(f"主指令文件缺少推荐节: {section}")
                
        # 检查代码示例
        code_blocks = re.findall(r'```[\s\S]*?```', content)
        if len(code_blocks) < 10:
            self.warnings.append("主指令文件代码示例较少，建议增加更多示例")
    
    def validate_configuration(self):
        """验证配置文件"""
        print("⚙️ 验证配置文件...")
        
        config_file = self.base_path / ".copilot" / "copilot.yml"
        if not config_file.exists():
            self.warnings.append("缺少中央配置文件: .copilot/copilot.yml")
            return
            
        try:
            with open(config_file, 'r', encoding='utf-8') as f:
                config = yaml.safe_load(f)
                
            # 验证配置结构
            required_keys = [
                "name", "description", "version",
                "project", "code_generation", "business_domain"
            ]
            
            for key in required_keys:
                if key not in config:
                    self.errors.append(f"配置文件缺少必需字段: {key}")
                    
        except yaml.YAMLError as e:
            self.errors.append(f"配置文件 YAML 格式错误: {e}")
        except Exception as e:
            self.errors.append(f"读取配置文件失败: {e}")
    
    def validate_schemas(self):
        """验证 JSON Schema 文件"""
        print("🔗 验证 Schema 文件...")
        
        schema_dir = self.base_path / ".copilot" / "schemas"
        if not schema_dir.exists():
            self.errors.append("缺少 schemas 目录")
            return
            
        required_schemas = [
            "entities.json",
            "api-responses.json"
        ]
        
        for schema_file in required_schemas:
            file_path = schema_dir / schema_file
            if not file_path.exists():
                self.errors.append(f"缺少 schema 文件: {schema_file}")
                continue
                
            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    schema = json.load(f)
                    
                # 验证 JSON Schema 结构
                if "$schema" not in schema:
                    self.warnings.append(f"Schema 文件 {schema_file} 缺少 $schema 字段")
                    
                if "definitions" not in schema and "properties" not in schema:
                    self.warnings.append(f"Schema 文件 {schema_file} 缺少 definitions 或 properties")
                    
            except json.JSONDecodeError as e:
                self.errors.append(f"Schema 文件 {schema_file} JSON 格式错误: {e}")
            except Exception as e:
                self.errors.append(f"读取 schema 文件 {schema_file} 失败: {e}")
    
    def validate_patterns(self):
        """验证模式文件"""
        print("🎨 验证模式文件...")
        
        patterns_dir = self.base_path / ".copilot" / "patterns"
        if not patterns_dir.exists():
            self.errors.append("缺少 patterns 目录")
            return
            
        required_patterns = [
            "coding-patterns.md",
            "api-patterns.md", 
            "database-patterns.md",
            "testing-patterns.md"
        ]
        
        for pattern_file in required_patterns:
            file_path = patterns_dir / pattern_file
            if not file_path.exists():
                self.errors.append(f"缺少模式文件: {pattern_file}")
                continue
                
            content = file_path.read_text(encoding='utf-8')
            
            # 检查代码示例数量
            code_blocks = re.findall(r'```[\s\S]*?```', content)
            if len(code_blocks) < 5:
                self.warnings.append(f"模式文件 {pattern_file} 代码示例较少")
                
            # 检查标题结构
            headings = re.findall(r'^#+\s+(.+)$', content, re.MULTILINE)
            if len(headings) < 5:
                self.warnings.append(f"模式文件 {pattern_file} 结构化程度较低")
    
    def validate_templates(self):
        """验证模板文件"""
        print("📄 验证模板文件...")
        
        templates_dir = self.base_path / ".copilot" / "templates"
        if not templates_dir.exists():
            self.errors.append("缺少 templates 目录")
            return
            
        template_files = list(templates_dir.glob("*.md"))
        if len(template_files) == 0:
            self.warnings.append("templates 目录为空，建议添加代码模板")
            return
            
        for template_file in template_files:
            content = template_file.read_text(encoding='utf-8')
            
            # 检查模板变量
            template_vars = re.findall(r'\{(\w+)\}', content)
            if len(template_vars) == 0:
                self.warnings.append(f"模板文件 {template_file.name} 可能缺少模板变量")
                
            # 检查使用说明
            if "使用说明" not in content and "Usage" not in content:
                self.warnings.append(f"模板文件 {template_file.name} 缺少使用说明")
    
    def generate_report(self) -> Dict[str, Any]:
        """生成验证报告"""
        print("\n📊 生成验证报告...")
        
        total_files = self.count_instruction_files()
        
        report = {
            "timestamp": "2023-12-01T10:30:00Z",
            "summary": {
                "total_files": total_files,
                "errors": len(self.errors),
                "warnings": len(self.warnings),
                "status": "PASS" if len(self.errors) == 0 else "FAIL"
            },
            "details": {
                "errors": self.errors,
                "warnings": self.warnings
            },
            "recommendations": self.generate_recommendations()
        }
        
        # 输出报告
        print(f"\n{'=' * 60}")
        print("📋 GitHub Copilot 指令系统验证报告")
        print(f"{'=' * 60}")
        print(f"📁 总文件数: {report['summary']['total_files']}")
        print(f"❌ 错误: {report['summary']['errors']}")
        print(f"⚠️  警告: {report['summary']['warnings']}")
        print(f"✅ 状态: {report['summary']['status']}")
        
        if self.errors:
            print(f"\n❌ 错误详情:")
            for error in self.errors:
                print(f"  - {error}")
                
        if self.warnings:
            print(f"\n⚠️  警告详情:")
            for warning in self.warnings:
                print(f"  - {warning}")
                
        if report['recommendations']:
            print(f"\n💡 改进建议:")
            for rec in report['recommendations']:
                print(f"  - {rec}")
        
        if report['summary']['status'] == "PASS":
            print(f"\n🎉 验证通过！Copilot 指令系统配置完整。")
        else:
            print(f"\n⚠️  验证未通过，请修复上述错误后重新验证。")
            
        return report
    
    def count_instruction_files(self) -> int:
        """统计指令文件数量"""
        count = 0
        
        # 主要指令文件
        if (self.base_path / ".github" / "copilot-instructions.md").exists():
            count += 1
            
        # .copilot 目录下的文件
        copilot_dir = self.base_path / ".copilot"
        if copilot_dir.exists():
            for pattern in ["**/*.md", "**/*.json", "**/*.yml", "**/*.yaml"]:
                count += len(list(copilot_dir.glob(pattern)))
                
        return count
    
    def generate_recommendations(self) -> List[str]:
        """生成改进建议"""
        recommendations = []
        
        if len(self.errors) == 0 and len(self.warnings) < 3:
            recommendations.append("系统配置良好，建议定期更新和维护指令文件")
            
        if len(self.warnings) > 5:
            recommendations.append("警告较多，建议逐步完善缺失的文档和示例")
            
        # 检查是否有前端相关的模式文件
        frontend_pattern = self.base_path / ".copilot" / "patterns" / "frontend-patterns.md"
        if not frontend_pattern.exists():
            recommendations.append("建议添加前端开发模式文件 (frontend-patterns.md)")
            
        # 检查是否有安全模式文件
        security_pattern = self.base_path / ".copilot" / "patterns" / "security-patterns.md"
        if not security_pattern.exists():
            recommendations.append("建议添加安全开发模式文件 (security-patterns.md)")
            
        return recommendations

def main():
    """主函数"""
    validator = CopilotSystemValidator()
    report = validator.validate_all()
    
    # 保存报告到文件
    report_file = Path(".copilot") / "validation-report.json"
    if report_file.parent.exists():
        with open(report_file, 'w', encoding='utf-8') as f:
            json.dump(report, f, indent=2, ensure_ascii=False)
        print(f"\n📄 验证报告已保存到: {report_file}")

if __name__ == "__main__":
    main()